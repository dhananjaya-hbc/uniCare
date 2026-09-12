using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniCare.Application.Features.Students;
using UniCare.Application.Features.Visits;
using UniCare.Application.Features.Visits.Dtos;
using UniCare.Domain.Constants;
using UniCare.Domain.Enums;

namespace UniCare.Api.Controllers;

/// <summary>
/// Check-in and the live queue. Split between a student-scoped route
/// (check-in) and visit-id-addressed routes (everything else), the same
/// shape as AppointmentsController.
/// </summary>
[ApiController]
[Authorize]
public class VisitsController(
    IVisitService visitService,
    IStudentService studentService) : ControllerBase
{
    private Guid CurrentApplicationUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsStaff() => AppRoles.Staff.Any(User.IsInRole);

    [HttpPost("api/students/{studentId:guid}/check-in")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<VisitDto>> CheckIn(
        Guid studentId, CheckInRequest request, CancellationToken cancellationToken)
    {
        var visit = await visitService.CheckInAsync(studentId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = visit.Id }, visit);
    }

    /// <summary>Staff-only: the live board for one stage.</summary>
    [HttpGet("api/visits/queue")]
    public async Task<ActionResult<IReadOnlyList<VisitDto>>> GetQueue(
        [FromQuery] QueueStage stage, CancellationToken cancellationToken)
    {
        if (!IsStaff())
        {
            return Forbid();
        }

        return Ok(await visitService.GetQueueAsync(stage, cancellationToken));
    }

    [HttpGet("api/visits/{id:guid}")]
    public async Task<ActionResult<VisitDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var visit = await visitService.GetByIdAsync(id, cancellationToken);
        if (visit is null)
        {
            return NotFound();
        }

        if (!IsStaff() &&
            !await studentService.IsOwnedByApplicationUserAsync(visit.StudentId, CurrentApplicationUserId, cancellationToken))
        {
            return Forbid();
        }

        return Ok(visit);
    }

    /// <summary>Nurse may call a Nurse-stage visit, Doctor a Doctor-stage one; Admin may call either.</summary>
    [HttpPost("api/visits/{id:guid}/call")]
    public async Task<ActionResult<VisitDto>> Call(Guid id, CancellationToken cancellationToken)
    {
        var visit = await visitService.GetByIdAsync(id, cancellationToken);
        if (visit is null)
        {
            return NotFound();
        }

        if (!CanActOnStage(visit.Stage))
        {
            return Forbid();
        }

        return Ok(await visitService.CallAsync(id, cancellationToken));
    }

    /// <summary>Same per-stage rule as Call.</summary>
    [HttpPost("api/visits/{id:guid}/advance")]
    public async Task<ActionResult<VisitDto>> Advance(Guid id, CancellationToken cancellationToken)
    {
        var visit = await visitService.GetByIdAsync(id, cancellationToken);
        if (visit is null)
        {
            return NotFound();
        }

        if (!CanActOnStage(visit.Stage))
        {
            return Forbid();
        }

        return Ok(await visitService.AdvanceAsync(id, cancellationToken));
    }

    [HttpPost("api/visits/{id:guid}/abandon")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<VisitDto>> Abandon(Guid id, CancellationToken cancellationToken) =>
        Ok(await visitService.AbandonAsync(id, cancellationToken));

    private bool CanActOnStage(QueueStage stage)
    {
        if (User.IsInRole(AppRoles.Admin)) return true;

        return stage switch
        {
            QueueStage.Nurse => User.IsInRole(AppRoles.Nurse),
            QueueStage.Doctor => User.IsInRole(AppRoles.Doctor),
            _ => false,
        };
    }
}
