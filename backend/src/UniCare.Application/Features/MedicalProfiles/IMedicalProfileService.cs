using UniCare.Application.Features.MedicalProfiles.Dtos;

namespace UniCare.Application.Features.MedicalProfiles;

public interface IMedicalProfileService
{
    Task<MedicalProfileDto?> GetByStudentIdAsync(
        Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>Creates the profile on first call, updates it thereafter.</summary>
    Task<MedicalProfileDto> UpsertAsync(
        Guid studentId, UpsertMedicalProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<MedicalProfileDto> SubmitAsync(
        Guid studentId, CancellationToken cancellationToken = default);

    Task<MedicalProfileDto> VerifyAsync(
        Guid studentId, Guid verifiedByStaffId, CancellationToken cancellationToken = default);

    Task<MedicalProfileDto> RejectAsync(
        Guid studentId, Guid rejectedByStaffId, string reason,
        CancellationToken cancellationToken = default);
}
