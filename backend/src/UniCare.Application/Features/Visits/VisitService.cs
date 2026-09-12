using Microsoft.EntityFrameworkCore;
using UniCare.Application.Abstractions;
using UniCare.Application.Exceptions;
using UniCare.Application.Features.Visits.Dtos;
using UniCare.Domain.Entities;
using UniCare.Domain.Enums;

namespace UniCare.Application.Features.Visits;

/// <summary>
/// Check-in and queue progression. QueueEntry has a unique index on
/// MedicalVisitId — one row per visit, mutated as it moves through stages —
/// so the queue number is assigned once at check-in and never recomputed;
/// only Stage, CalledAt and CompletedAt change as the visit progresses.
/// </summary>
public class VisitService(IApplicationDbContext db) : IVisitService
{
    public async Task<VisitDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.QueueEntries
            .AsNoTracking()
            .Where(q => q.MedicalVisitId == id)
            .Select(VisitMappings.Projection)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<VisitDto>> GetQueueAsync(
        QueueStage stage, CancellationToken cancellationToken = default) =>
        await db.QueueEntries
            .AsNoTracking()
            .Where(q => q.Stage == stage && q.CompletedAt == null)
            .OrderBy(q => q.QueueNumber)
            .Select(VisitMappings.Projection)
            .ToListAsync(cancellationToken);

    public async Task<VisitDto> CheckInAsync(
        Guid studentId, CheckInRequest request, CancellationToken cancellationToken = default)
    {
        var studentExists = await db.Students.AnyAsync(s => s.Id == studentId, cancellationToken);
        if (!studentExists)
        {
            throw new NotFoundException(nameof(Student), studentId);
        }

        var todayStart = new DateTimeOffset(DateOnly.FromDateTime(DateTime.UtcNow), TimeOnly.MinValue, TimeSpan.Zero);
        var todayEnd = todayStart.AddDays(1);

        var hasOpenVisitToday = await db.MedicalVisits.AnyAsync(v =>
            v.StudentId == studentId &&
            v.CheckedInAt >= todayStart && v.CheckedInAt < todayEnd &&
            v.Status != VisitStatus.Completed && v.Status != VisitStatus.Abandoned,
            cancellationToken);

        if (hasOpenVisitToday)
        {
            throw new ConflictException("This student already has an open visit today.");
        }

        if (request.AppointmentId is { } appointmentId)
        {
            var appointment = await db.Appointments.FirstOrDefaultAsync(
                a => a.Id == appointmentId, cancellationToken)
                ?? throw new NotFoundException(nameof(Appointment), appointmentId);

            if (appointment.StudentId != studentId)
            {
                throw new ConflictException("This appointment does not belong to this student.");
            }

            if (appointment.Status != AppointmentStatus.Approved)
            {
                throw new ConflictException(
                    $"Only an approved appointment can be checked in; this one is {appointment.Status}.");
            }

            if (appointment.ScheduledDate != DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new ConflictException("This appointment is not scheduled for today.");
            }

            var alreadyCheckedIn = await db.MedicalVisits.AnyAsync(
                v => v.AppointmentId == appointmentId, cancellationToken);

            if (alreadyCheckedIn)
            {
                throw new ConflictException("This appointment has already been checked in.");
            }
        }

        var now = DateTimeOffset.UtcNow;

        var queueNumber = await db.QueueEntries.CountAsync(
            q => q.EnteredAt >= todayStart && q.EnteredAt < todayEnd, cancellationToken) + 1;

        var visit = new MedicalVisit
        {
            StudentId = studentId,
            AppointmentId = request.AppointmentId,
            CheckedInAt = now,
            Status = VisitStatus.CheckedIn,
            IsEmergency = request.IsEmergency,
        };

        var queueEntry = new QueueEntry
        {
            MedicalVisit = visit,
            QueueNumber = queueNumber,
            Stage = QueueStage.Nurse,
            EnteredAt = now,
        };

        db.MedicalVisits.Add(visit);
        db.QueueEntries.Add(queueEntry);
        await db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(visit.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(MedicalVisit), visit.Id);
    }

    public async Task<VisitDto> CallAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var (_, queueEntry) = await LoadAsync(id, cancellationToken);

        queueEntry.CalledAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException(nameof(MedicalVisit), id);
    }

    public async Task<VisitDto> AdvanceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var (visit, queueEntry) = await LoadAsync(id, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        switch (queueEntry.Stage)
        {
            case QueueStage.Nurse:
                queueEntry.Stage = QueueStage.Doctor;
                queueEntry.CalledAt = null;
                queueEntry.EnteredAt = now;
                visit.Status = VisitStatus.AwaitingDoctor;
                break;

            case QueueStage.Doctor:
                queueEntry.CompletedAt = now;
                visit.Status = VisitStatus.Completed;
                visit.CompletedAt = now;
                break;

            default:
                throw new ConflictException(
                    $"Cannot advance a visit at the {queueEntry.Stage} stage yet.");
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException(nameof(MedicalVisit), id);
    }

    public async Task<VisitDto> AbandonAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var (visit, queueEntry) = await LoadAsync(id, cancellationToken);

        if (visit.Status is VisitStatus.Completed or VisitStatus.Abandoned)
        {
            throw new ConflictException($"This visit is already {visit.Status}.");
        }

        var now = DateTimeOffset.UtcNow;
        visit.Status = VisitStatus.Abandoned;
        visit.CompletedAt = now;
        queueEntry.CompletedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException(nameof(MedicalVisit), id);
    }

    private async Task<(MedicalVisit Visit, QueueEntry QueueEntry)> LoadAsync(
        Guid id, CancellationToken cancellationToken)
    {
        var visit = await db.MedicalVisits
            .Include(v => v.QueueEntry)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(MedicalVisit), id);

        var queueEntry = visit.QueueEntry
            ?? throw new ConflictException("This visit has no active queue entry.");

        return (visit, queueEntry);
    }
}
