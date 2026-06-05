using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Queries;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;
using VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Transform;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST;

[Authorize]
[RequiresPlus]
[ApiController]
[Route("api/v1/iot-monitoring")]
[Produces(MediaTypeNames.Application.Json)]
public class BovineHealthRecordController(
    IBovineHealthRecordQueryService queryService)
    : ControllerBase
{
    // NOTE: telemetry ingestion is NOT exposed over HTTP. Per constraint CON2 the
    // collar communicates exclusively over MQTT, so readings arrive through
    // MqttTelemetryConsumer. This controller only serves read queries to the apps.

    /// <summary>
    /// Returns the full telemetry history for a bovine. Requires JWT.
    /// </summary>
    [HttpGet("bovines/{bovineId}/records")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BovineHealthRecordResource>>> GetRecordsByBovineId(
        [FromRoute] int bovineId)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        var records = await queryService.Handle(new GetHealthRecordsByBovineIdQuery(bovineId, user.Id));
        var resources = records.Select(BovineHealthRecordResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /// <summary>
    /// Returns the most recent reading for a bovine. Requires JWT.
    /// </summary>
    [HttpGet("bovines/{bovineId}/latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BovineHealthRecordResource>> GetLatestByBovineId(
        [FromRoute] int bovineId)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        var record = await queryService.Handle(new GetLatestHealthRecordByBovineIdQuery(bovineId, user.Id));
        if (record is null) return NotFound();
        return Ok(BovineHealthRecordResourceFromEntityAssembler.ToResourceFromEntity(record));
    }
}
