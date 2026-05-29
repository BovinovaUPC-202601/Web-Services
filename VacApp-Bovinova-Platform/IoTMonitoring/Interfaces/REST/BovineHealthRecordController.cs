using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Queries;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;
using VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Transform;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST;

[ApiController]
[Route("api/v1/iot-monitoring")]
[Produces(MediaTypeNames.Application.Json)]
public class BovineHealthRecordController(
    IBovineHealthRecordCommandService commandService,
    IBovineHealthRecordQueryService   queryService)
    : ControllerBase
{
    /// <summary>
    /// Receives a telemetry reading from an ESP32 device.
    /// AllowAnonymous — ESP32 cannot use JWT.
    /// Returns alarm flag so the device can activate its LED actuator.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("telemetry")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TelemetryResponseResource>> PostTelemetry(
        [FromBody] CreateBovineHealthRecordResource resource)
    {
        var command = CreateBovineHealthRecordCommandFromResourceAssembler.ToCommandFromResource(resource);
        var record  = await commandService.Handle(command);

        if (record is null) return BadRequest("Could not save telemetry record.");

        var response = new TelemetryResponseResource(
            record.Id,
            record.IsAlert,
            record.IsAlert
                ? "ALERT: vital signs outside normal bovine range."
                : "OK: vital signs within normal range.");

        return CreatedAtAction(nameof(GetLatestByBovineId),
            new { bovineId = record.BovineId }, response);
    }

    /// <summary>
    /// Returns the full telemetry history for a bovine. Requires JWT.
    /// </summary>
    [Authorize]
    [HttpGet("bovines/{bovineId}/records")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BovineHealthRecordResource>>> GetRecordsByBovineId(
        [FromRoute] int bovineId)
    {
        var records = await queryService.Handle(new GetHealthRecordsByBovineIdQuery(bovineId));
        var resources = records.Select(BovineHealthRecordResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /// <summary>
    /// Returns the most recent reading for a bovine. Requires JWT.
    /// </summary>
    [Authorize]
    [HttpGet("bovines/{bovineId}/latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BovineHealthRecordResource>> GetLatestByBovineId(
        [FromRoute] int bovineId)
    {
        var record = await queryService.Handle(new GetLatestHealthRecordByBovineIdQuery(bovineId));
        if (record is null) return NotFound();
        return Ok(BovineHealthRecordResourceFromEntityAssembler.ToResourceFromEntity(record));
    }
}
