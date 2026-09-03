using System.Linq.Expressions;
using UniCare.Domain.Entities;

namespace UniCare.Application.Features.MedicalProfiles.Dtos;

public static class MedicalProfileMappings
{
    public static Expression<Func<MedicalProfile, MedicalProfileDto>> Projection =>
        profile => new MedicalProfileDto
        {
            Id = profile.Id,
            StudentId = profile.StudentId,
            BloodGroup = profile.BloodGroup,
            HeightCm = profile.HeightCm,
            WeightKg = profile.WeightKg,
            ChronicConditions = profile.ChronicConditions,
            Allergies = profile.Allergies,
            CurrentMedications = profile.CurrentMedications,
            EyeExamination = profile.EyeExamination,
            DentalExamination = profile.DentalExamination,
            Status = profile.Status,
            SubmittedAt = profile.SubmittedAt,
            VerifiedAt = profile.VerifiedAt,
            RejectionReason = profile.RejectionReason,
        };

    public static MedicalProfileDto ToDto(this MedicalProfile profile) =>
        Projection.Compile()(profile);
}
