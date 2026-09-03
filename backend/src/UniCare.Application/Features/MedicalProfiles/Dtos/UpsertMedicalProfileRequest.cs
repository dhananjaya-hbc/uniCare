using UniCare.Domain.Enums;

namespace UniCare.Application.Features.MedicalProfiles.Dtos;

/// <summary>
/// The clinical fields a student may edit. Status, SubmittedAt, VerifiedAt and
/// VerifiedByStaffId are absent on purpose — those are workflow outcomes, and a
/// client that cannot name them cannot forge a verified profile.
/// </summary>
public record UpsertMedicalProfileRequest
{
    public required BloodGroup BloodGroup { get; init; }
    public decimal? HeightCm { get; init; }
    public decimal? WeightKg { get; init; }
    public string? ChronicConditions { get; init; }
    public string? Allergies { get; init; }
    public string? CurrentMedications { get; init; }
    public string? EyeExamination { get; init; }
    public string? DentalExamination { get; init; }
}
