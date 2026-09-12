namespace UniCare.Application.Features.Visits.Dtos;

/// <summary>
/// AppointmentId is null for a walk-in or emergency. No validator exists for
/// this type — every combination of these two fields is valid; the service
/// checks the appointment's own state (exists, belongs to this student,
/// Approved, scheduled for today) rather than a shape rule.
/// </summary>
public record CheckInRequest
{
    public Guid? AppointmentId { get; init; }
    public bool IsEmergency { get; init; }
}
