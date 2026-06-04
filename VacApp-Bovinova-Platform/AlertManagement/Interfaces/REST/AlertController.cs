using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Services;
using VacApp_Bovinova_Platform.AlertManagement.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.AlertManagement.Interfaces.REST.Transform;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;

namespace VacApp_Bovinova_Platform.AlertManagement.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/alerts")]
[Produces(MediaTypeNames.Application.Json)]
public class AlertController(
    IAlertCommandService commandService,
    IAlertQueryService   queryService)
    : ControllerBase
{
    /// <summary>
    /// Returns all alerts for the authenticated rancher ordered by most recent first.
    /// The owner is resolved from the JWT — never from client input.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AlertResource>>> GetAlertsByUserId()
    {
        if (HttpContext.Items["User"] is not User user)
            return Unauthorized("User not found in context.");

        var alerts    = await queryService.Handle(new GetAlertsByUserIdQuery(user.Id));
        var resources = alerts.Select(AlertResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /// <summary>
    /// Returns a single alert by its ID, only if it belongs to the authenticated rancher.
    /// </summary>
    [HttpGet("{alertId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertResource>> GetAlertById([FromRoute] int alertId)
    {
        if (HttpContext.Items["User"] is not User user)
            return Unauthorized("User not found in context.");

        var alert = await queryService.Handle(new GetAlertByIdQuery(alertId));
        // Return NotFound (not Forbidden) when the alert belongs to another user,
        // so the endpoint does not leak the existence of foreign alerts.
        if (alert is null || alert.UserId != user.Id) return NotFound();
        return Ok(AlertResourceFromEntityAssembler.ToResourceFromEntity(alert));
    }

    /// <summary>
    /// Marks an alert as read, only if it belongs to the authenticated rancher.
    /// </summary>
    [HttpPut("{alertId}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertResource>> MarkAsRead([FromRoute] int alertId)
    {
        if (HttpContext.Items["User"] is not User user)
            return Unauthorized("User not found in context.");

        // Verify ownership before mutating — prevents marking another user's alert as read.
        var existing = await queryService.Handle(new GetAlertByIdQuery(alertId));
        if (existing is null || existing.UserId != user.Id) return NotFound();

        var alert = await commandService.Handle(new MarkAlertAsReadCommand(alertId));
        if (alert is null) return NotFound();
        return Ok(AlertResourceFromEntityAssembler.ToResourceFromEntity(alert));
    }
}
