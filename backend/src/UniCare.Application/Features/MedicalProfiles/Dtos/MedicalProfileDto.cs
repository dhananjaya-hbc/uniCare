using UniCare.Domain.Enums;

namespace UniCare.Application.Features.MedicalProfiles.Dtos;

public record MedicalProfileDto
{
    public required Guid Id { get; init; }
    public required Guid StudentId { get; init; }

    public required BloodGroup BloodGroup { get; init; }
    public decimal? HeightCm { get; init; }
    public decimal? WeightKg { get; init; }
    public string? ChronicConditions { get; init; }
    public string? Allergies { get; init; }
    public string? CurrentMedications { get; init; }
    public string? EyeExamination { get; init; }
    public string? DentalExamination { get; init; }

    public required VerificationStatus Status { get; init; }
    public DateTimeOffset? SubmittedAt { get; init; }
    public DateTimeOffset? VerifiedAt { get; init; }
    public string? RejectionReason { get; init; }
}
