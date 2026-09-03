namespace UniCare.Application.Features.MedicalProfiles.Dtos;

public record RejectMedicalProfileRequest
{
    public required string Reason { get; init; }
}
