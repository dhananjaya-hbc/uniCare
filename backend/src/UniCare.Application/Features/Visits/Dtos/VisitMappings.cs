using System.Linq.Expressions;
using UniCare.Domain.Entities;

namespace UniCare.Application.Features.Visits.Dtos;

public static class VisitMappings
{
    /// <summary>
    /// Projects from QueueEntry rather than MedicalVisit — QueueEntry.MedicalVisit
    /// and MedicalVisit.Student are both required navigations, so every field here
    /// is guaranteed non-null, unlike MedicalVisit.QueueEntry which is optional on
    /// the entity even though CheckInAsync always creates one alongside it.
    /// </summary>
    public static Expression<Func<QueueEntry, VisitDto>> Projection =>
        q => new VisitDto
        {
            Id = q.MedicalVisit.Id,
            StudentId = q.MedicalVisit.StudentId,
            StudentName = q.MedicalVisit.Student.FullName,
            AppointmentId = q.MedicalVisit.AppointmentId,
            CheckedInAt = q.MedicalVisit.CheckedInAt,
            CompletedAt = q.MedicalVisit.CompletedAt,
            Status = q.MedicalVisit.Status,
            IsEmergency = q.MedicalVisit.IsEmergency,
            QueueNumber = q.QueueNumber,
            Stage = q.Stage,
            CalledAt = q.CalledAt,
        };
}
