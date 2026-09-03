using FluentValidation;
using UniCare.Application.Features.MedicalProfiles.Dtos;

namespace UniCare.Application.Features.MedicalProfiles.Validators;

public class RejectMedicalProfileRequestValidator : AbstractValidator<RejectMedicalProfileRequest>
{
    public RejectMedicalProfileRequestValidator()
    {
        // A rejection without a reason leaves the student with nothing to act on.
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("A rejection reason is required.")
            .MaximumLength(1000);
    }
}
