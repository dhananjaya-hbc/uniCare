using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using UniCare.Application.Features.MedicalProfiles;
using UniCare.Application.Features.MedicalProfiles.Dtos;

namespace UniCare.Api.Controllers;

/// <summary>
/// A student has at most one medical profile, so the routes hang off the student
/// rather than exposing a separate profile id.
/// </summary>
[ApiController]
[Route("api/students/{studentId:guid}/medical-profile")]
public class MedicalProfilesController(
    IMedicalProfileService profileService,
    IValidator<UpsertMedicalProfileRequest> upsertValidator,
    IValidator<RejectMedicalProfileRequest> rejectValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MedicalProfileDto>> Get(
        Guid studentId, CancellationToken cancellationToken)
    {
        var profile = await profileService.GetByStudentIdAsync(studentId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    /// <summary>Creates the profile on first call, updates it thereafter.</summary>
    [HttpPut]
    public async Task<ActionResult<MedicalProfileDto>> Upsert(
        Guid studentId, UpsertMedicalProfileRequest request, CancellationToken cancellationToken)
    {
        var validation = await upsertValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return ValidationProblem(ModelState);
        }

        return Ok(await profileService.UpsertAsync(studentId, request, cancellationToken));
    }

    [HttpPost("submit")]
    public async Task<ActionResult<MedicalProfileDto>> Submit(
        Guid studentId, CancellationToken cancellationToken)
    {
        return Ok(await profileService.SubmitAsync(studentId, cancellationToken));
    }

    // TODO(auth): the reviewing staff id must come from the JWT, not the caller.
    // Until then it is passed explicitly so the workflow can be exercised.
    [HttpPost("verify")]
    public async Task<ActionResult<MedicalProfileDto>> Verify(
        Guid studentId, [FromQuery] Guid staffId, CancellationToken cancellationToken)
    {
        return Ok(await profileService.VerifyAsync(studentId, staffId, cancellationToken));
    }

    [HttpPost("reject")]
    public async Task<ActionResult<MedicalProfileDto>> Reject(
        Guid studentId,
        [FromQuery] Guid staffId,
        RejectMedicalProfileRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await rejectValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return ValidationProblem(ModelState);
        }

        return Ok(await profileService.RejectAsync(
            studentId, staffId, request.Reason, cancellationToken));
    }
}
