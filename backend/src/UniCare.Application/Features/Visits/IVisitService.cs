using UniCare.Application.Features.Visits.Dtos;
using UniCare.Domain.Enums;

namespace UniCare.Application.Features.Visits;

public interface IVisitService
{
    Task<VisitDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The live board for one stage — active entries only, ordered by queue number.</summary>
    Task<IReadOnlyList<VisitDto>> GetQueueAsync(
        QueueStage stage, CancellationToken cancellationToken = default);

    /// <exception cref="Exceptions.NotFoundException">Student, or the given appointment, does not exist.</exception>
    /// <exception cref="Exceptions.ConflictException">
    /// The student already has an open visit today, or the appointment is not an
    /// Approved appointment scheduled for today.
    /// </exception>
    Task<VisitDto> CheckInAsync(
        Guid studentId, CheckInRequest request, CancellationToken cancellationToken = default);

    /// <summary>Marks the queue entry as called. Safe to call again to re-call.</summary>
    Task<VisitDto> CallAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Nurse stage advances to Doctor; Doctor stage completes the visit.</summary>
    Task<VisitDto> AdvanceAsync(Guid id, CancellationToken cancellationToken = default);

    Task<VisitDto> AbandonAsync(Guid id, CancellationToken cancellationToken = default);
}
