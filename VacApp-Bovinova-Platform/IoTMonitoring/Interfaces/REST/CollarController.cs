using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using VacApp_Bovinova_Platform.IoTMonitoring.Application.ACL;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Queries;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;
using VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Transform;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST;

/// <summary>
/// Collar registration, status and capacity. The whole controller requires the
/// Plus plan ([RequiresPlus]); Free users have no access to collars at all.
/// </summary>
[Authorize]
[RequiresPlus]
[ApiController]
[Route("api/v1/iot-monitoring/collars")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Collars")]
public class CollarController(
    ICollarCommandService commandService,
    ICollarQueryService queryService,
    ISubscriptionContextFacade subscriptionContext,
    IBovineHealthRecordQueryService healthRecordQueryService)
    : ControllerBase
{
    /// <summary>Registers a collar (ESP32 device) and assigns it to a bovine.</summary>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created, "Collar registered", typeof(CollarResource))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Device already registered")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Allowance reached or bovine not owned")]
    public async Task<IActionResult> RegisterCollar([FromBody] RegisterCollarResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        try
        {
            var command = new RegisterCollarCommand(user.Id, resource.DeviceId, resource.BovineId);
            var collar = await commandService.Handle(command);
            return CreatedAtAction(nameof(GetMyCollars),
                CollarResourceFromEntityAssembler.ToResourceFromEntity(collar));
        }
        catch (DuplicateCollarException e)
        {
            return Conflict(e.Message);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>Lists the user's collars with operational/lifecycle status and last reading (TP).</summary>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "Collar statuses", typeof(IEnumerable<CollarStatusResource>))]
    public async Task<IActionResult> GetMyCollars()
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        var collars = await queryService.Handle(new GetCollarsByUserIdQuery(user.Id));

        var statuses = new List<CollarStatusResource>();
        foreach (var collar in collars)
        {
            var latest = await healthRecordQueryService.Handle(
                new GetLatestHealthRecordByBovineIdQuery(collar.BovineId, user.Id));

            statuses.Add(new CollarStatusResource(
                collar.Id,
                collar.DeviceId,
                collar.BovineId,
                collar.ResolveOperationalStatus(latest?.RecordedAt).ToString(),
                collar.LifecycleStatus.ToString(),
                latest?.Temperature,
                latest?.HeartRate,
                latest?.BatteryLevel,
                latest?.RecordedAt,
                collar.RegisteredAt));
        }

        return Ok(statuses);
    }

    /// <summary>Returns current collar usage and allowance for the authenticated user.</summary>
    [HttpGet("capacity")]
    [SwaggerResponse(StatusCodes.Status200OK, "Collar capacity", typeof(CollarCapacityResource))]
    public async Task<IActionResult> GetCapacity()
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        var active = await queryService.GetActiveCountAsync(user.Id);
        var allowance = await subscriptionContext.GetCollarAllowanceAsync(user.Id);
        return Ok(new CollarCapacityResource(active, allowance, Math.Max(0, allowance - active)));
    }
}
