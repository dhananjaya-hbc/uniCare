using Microsoft.EntityFrameworkCore;
using UniCare.Application.Abstractions;
using UniCare.Application.Exceptions;
using UniCare.Application.Features.MedicalProfiles.Dtos;
using UniCare.Domain.Entities;
using UniCare.Domain.Enums;

namespace UniCare.Application.Features.MedicalProfiles;

/// <summary>
/// Owns the medical-profile workflow: Draft → Submitted → Verified or Rejected.
/// Every transition is checked here, because the entity's setters are public and
/// nothing else stops a caller writing an invalid state.
/// </summary>
public class MedicalProfileService(IApplicationDbContext db) : IMedicalProfileService
{
    /// <summary>States in which a student may still edit their own data.</summary>
    private static readonly VerificationStatus[] EditableStates =
        [VerificationStatus.Draft, VerificationStatus.Rejected];

    public async Task<MedicalProfileDto?> GetByStudentIdAsync(
        Guid studentId, CancellationToken cancellationToken = default) =>
        await db.MedicalProfiles
            .AsNoTracking()
            .Where(p => p.StudentId == studentId)
            .Select(MedicalProfileMappings.Projection)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<MedicalProfileDto> UpsertAsync(
        Guid studentId,
        UpsertMedicalProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var studentExists = await db.Students
            .AnyAsync(s => s.Id == studentId, cancellationToken);

        if (!studentExists)
        {
            throw new NotFoundException(nameof(Student), studentId);
        }

        var profile = await db.MedicalProfiles
            .FirstOrDefaultAsync(p => p.StudentId == studentId, cancellationToken);

        if (profile is null)
        {
            // First save creates the profile as a draft.
            profile = new MedicalProfile
            {
                StudentId = studentId,
                Status = VerificationStatus.Draft,
            };
            db.MedicalProfiles.Add(profile);
        }
        else if (!EditableStates.Contains(profile.Status))
        {
            throw new ConflictException(
                $"This medical profile cannot be edited while it is {profile.Status}.");
        }

        Apply(request, profile);

        // Editing after a rejection returns the profile to Draft and clears the
        // stale reason, so the student is not still shown why a previous version
        // was rejected.
        if (profile.Status == VerificationStatus.Rejected)
        {
            profile.Status = VerificationStatus.Draft;
            profile.RejectionReason = null;
        }

        await db.SaveChangesAsync(cancellationToken);
        return profile.ToDto();
    }

    public async Task<MedicalProfileDto> SubmitAsync(
        Guid studentId, CancellationToken cancellationToken = default)
    {
        var profile = await LoadAsync(studentId, cancellationToken);

        if (!EditableStates.Contains(profile.Status))
        {
            throw new ConflictException(
                $"Only a draft or rejected profile can be submitted; this one is {profile.Status}.");
        }

        profile.Status = VerificationStatus.SubmittedForVerification;
        profile.SubmittedAt = DateTimeOffset.UtcNow;
        profile.RejectionReason = null;

        await db.SaveChangesAsync(cancellationToken);
        return profile.ToDto();
    }

    public async Task<MedicalProfileDto> VerifyAsync(
        Guid studentId, Guid verifiedByStaffId, CancellationToken cancellationToken = default)
    {
        var profile = await LoadAsync(studentId, cancellationToken);
        EnsureAwaitingReview(profile, "verified");

        profile.Status = VerificationStatus.Verified;
        profile.VerifiedAt = DateTimeOffset.UtcNow;
        profile.VerifiedByStaffId = verifiedByStaffId;
        profile.RejectionReason = null;

        await db.SaveChangesAsync(cancellationToken);
        return profile.ToDto();
    }

    public async Task<MedicalProfileDto> RejectAsync(
        Guid studentId, Guid rejectedByStaffId, string reason,
        CancellationToken cancellationToken = default)
    {
        var profile = await LoadAsync(studentId, cancellationToken);
        EnsureAwaitingReview(profile, "rejected");

        profile.Status = VerificationStatus.Rejected;
        profile.RejectionReason = reason.Trim();
        // Records who reviewed it, whichever way the decision went.
        profile.VerifiedByStaffId = rejectedByStaffId;
        profile.VerifiedAt = null;

        await db.SaveChangesAsync(cancellationToken);
        return profile.ToDto();
    }

    private async Task<MedicalProfile> LoadAsync(Guid studentId, CancellationToken cancellationToken) =>
        await db.MedicalProfiles
            .FirstOrDefaultAsync(p => p.StudentId == studentId, cancellationToken)
            ?? throw new NotFoundException(nameof(MedicalProfile), studentId);

    private static void EnsureAwaitingReview(MedicalProfile profile, string action)
    {
        if (profile.Status != VerificationStatus.SubmittedForVerification)
        {
            throw new ConflictException(
                $"Only a profile awaiting verification can be {action}; this one is {profile.Status}.");
        }
    }

    private static void Apply(UpsertMedicalProfileRequest request, MedicalProfile profile)
    {
        profile.BloodGroup = request.BloodGroup;
        profile.HeightCm = request.HeightCm;
        profile.WeightKg = request.WeightKg;
        profile.ChronicConditions = request.ChronicConditions?.Trim();
        profile.Allergies = request.Allergies?.Trim();
        profile.CurrentMedications = request.CurrentMedications?.Trim();
        profile.EyeExamination = request.EyeExamination?.Trim();
        profile.DentalExamination = request.DentalExamination?.Trim();
    }
}
    