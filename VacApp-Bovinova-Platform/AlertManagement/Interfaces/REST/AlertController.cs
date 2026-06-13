using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Services;
using VacApp_Bovinova_Platform.AlertManagement.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.AlertManagement.Interfaces.REST.Transform;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;

namespace VacApp_Bovinova_Platform.AlertManagement.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/alerts")]
[Produces(MediaTypeNames.Application.Json)]
public class AlertController(
    IAlertCommandService commandService,
    IAlertQueryService   queryService,
    IStaffAccessService  staffAccessService)
    : ControllerBase
{
    /// <summary>
    /// Returns all alerts of the effective ranch owner ordered by most recent first.
    /// The owner is resolved from the JWT plus the staff-access table — never from client input.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AlertResource>>> GetAlertsByUserId()
    {
        if (HttpContext.Items["User"] is not User user)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var alerts    = await queryService.Handle(new GetAlertsByUserIdQuery(effectiveUserId));
        var resources = alerts.Select(AlertResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /// <summary>
    /// Returns a single alert by its ID, only if it belongs to the effective ranch owner.
    /// </summary>
    [HttpGet("{alertId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertResource>> GetAlertById([FromRoute] int alertId)
    {
        if (HttpContext.Items["User"] is not User user)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var alert = await queryService.Handle(new GetAlertByIdQuery(alertId));
        // Return NotFound (not Forbidden) when the alert belongs to another user,
        // so the endpoint does not leak the existence of foreign alerts.
        if (alert is null || alert.UserId != effectiveUserId) return NotFound();
        return Ok(AlertResourceFromEntityAssembler.ToResourceFromEntity(alert));
    }

    /// <summary>
    /// Marks an alert as read, only if it belongs to the effective ranch owner.
    /// Requires edit permission (Editor, Manager or Owner).
    /// </summary>
    [HttpPut("{alertId}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertResource>> MarkAsRead([FromRoute] int alertId)
    {
        if (HttpContext.Items["User"] is not User user)
            return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user))
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "Read-only staff cannot modify alerts." });
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        // Verify ownership before mutating — prevents marking another user's alert as read.
        var existing = await queryService.Handle(new GetAlertByIdQuery(alertId));
        if (existing is null || existing.UserId != effectiveUserId) return NotFound();

        var alert = await commandService.Handle(new MarkAlertAsReadCommand(alertId));
        if (alert is null) return NotFound();
        return Ok(AlertResourceFromEntityAssembler.ToResourceFromEntity(alert));
    }
}
