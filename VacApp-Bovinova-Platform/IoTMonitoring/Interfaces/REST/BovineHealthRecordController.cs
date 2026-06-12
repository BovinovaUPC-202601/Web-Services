using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Queries;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;
using VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Transform;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST;

[Authorize]
[RequiresPlus]
[ApiController]
[Route("api/v1/iot-monitoring")]
[Produces(MediaTypeNames.Application.Json)]
public class BovineHealthRecordController(
    IBovineHealthRecordQueryService queryService,
    IStaffAccessService staffAccessService)
    : ControllerBase
{
    // NOTE: telemetry ingestion is NOT exposed over HTTP. Per constraint CON2 the
    // collar communicates exclusively over MQTT, so readings arrive through
    // MqttTelemetryConsumer. This controller only serves read queries to the apps.

    /// <summary>
    /// Returns the full telemetry history for a bovine of the effective ranch owner. Requires JWT.
    /// </summary>
    [HttpGet("bovines/{bovineId}/records")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BovineHealthRecordResource>>> GetRecordsByBovineId(
        [FromRoute] int bovineId)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var records = await queryService.Handle(new GetHealthRecordsByBovineIdQuery(bovineId, effectiveUserId));
        var resources = records.Select(BovineHealthRecordResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /// <summary>
    /// Returns the most recent reading for a bovine of the effective ranch owner. Requires JWT.
    /// </summary>
    [HttpGet("bovines/{bovineId}/latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BovineHealthRecordResource>> GetLatestByBovineId(
        [FromRoute] int bovineId)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var record = await queryService.Handle(new GetLatestHealthRecordByBovineIdQuery(bovineId, effectiveUserId));
        if (record is null) return NotFound();
        return Ok(BovineHealthRecordResourceFromEntityAssembler.ToResourceFromEntity(record));
    }
}
