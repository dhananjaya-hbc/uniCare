using FluentValidation;
using UniCare.Application.Features.MedicalProfiles.Dtos;

namespace UniCare.Application.Features.MedicalProfiles.Validators;

public class UpsertMedicalProfileRequestValidator : AbstractValidator<UpsertMedicalProfileRequest>
{
    public UpsertMedicalProfileRequestValidator()
    {
        RuleFor(x => x.BloodGroup).IsInEnum();

        // Bounds are deliberately wide — the point is to catch a misplaced decimal
        // point or a value entered in the wrong unit, not to judge a body.
        RuleFor(x => x.HeightCm)
            .InclusiveBetween(50m, 250m)
            .When(x => x.HeightCm.HasValue)
            .WithMessage("Height must be between 50 and 250 cm.");

        RuleFor(x => x.WeightKg)
            .InclusiveBetween(10m, 300m)
            .When(x => x.WeightKg.HasValue)
            .WithMessage("Weight must be between 10 and 300 kg.");

        // These map to columns configured at 2000/1000 in MedicalProfileConfiguration.
        RuleFor(x => x.ChronicConditions).MaximumLength(2000);
        RuleFor(x => x.Allergies).MaximumLength(2000);
        RuleFor(x => x.CurrentMedications).MaximumLength(2000);
        RuleFor(x => x.EyeExamination).MaximumLength(1000);
        RuleFor(x => x.DentalExamination).MaximumLength(1000);
    }
}
