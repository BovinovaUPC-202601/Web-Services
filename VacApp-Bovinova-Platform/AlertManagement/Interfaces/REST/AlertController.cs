using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Services;
using VacApp_Bovinova_Platform.AlertManagement.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.AlertManagement.Interfaces.REST.Transform;
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
    /// Returns all alerts for a rancher ordered by most recent first.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AlertResource>>> GetAlertsByUserId([FromQuery] int userId)
    {
        var alerts    = await queryService.Handle(new GetAlertsByUserIdQuery(userId));
        var resources = alerts.Select(AlertResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /// <summary>
    /// Returns a single alert by its ID.
    /// </summary>
    [HttpGet("{alertId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertResource>> GetAlertById([FromRoute] int alertId)
    {
        var alert = await queryService.Handle(new GetAlertByIdQuery(alertId));
        if (alert is null) return NotFound();
        return Ok(AlertResourceFromEntityAssembler.ToResourceFromEntity(alert));
    }

    /// <summary>
    /// Marks an alert as read.
    /// </summary>
    [HttpPut("{alertId}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertResource>> MarkAsRead([FromRoute] int alertId)
    {
        var alert = await commandService.Handle(new MarkAlertAsReadCommand(alertId));
        if (alert is null) return NotFound();
        return Ok(AlertResourceFromEntityAssembler.ToResourceFromEntity(alert));
    }
}
