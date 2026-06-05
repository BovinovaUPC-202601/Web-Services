using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Queries;
using VacApp_Bovinova_Platform.IAM.Domain.Services;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using VacApp_Bovinova_Platform.IoTMonitoring.Application.ACL;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Queries;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;
using VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST;

/// <summary>
/// Admin-only collar recovery report: collars belonging to users whose subscription is
/// suspended or cancelled, so admins can chase device returns (TP US033/TS024).
/// </summary>
[Authorize]
[RequiresAdmin]
[ApiController]
[Route("api/v1/iot/devices")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Collar Recovery")]
public class CollarRecoveryController(
    ISubscriptionContextFacade subscriptionContext,
    ICollarQueryService collarQueryService,
    IUserQueryService userQueryService)
    : ControllerBase
{
    [HttpGet("recovery-report")]
    [SwaggerResponse(StatusCodes.Status200OK, "Recovery report", typeof(IEnumerable<CollarRecoveryResource>))]
    public async Task<IActionResult> GetRecoveryReport()
    {
        var inactive = await subscriptionContext.GetInactiveSubscriptionsAsync();

        var rows = new List<CollarRecoveryResource>();
        foreach (var sub in inactive)
        {
            var user = await userQueryService.Handle(new GetUserByIdQuery(sub.UserId));
            var collars = await collarQueryService.Handle(new GetCollarsByUserIdQuery(sub.UserId));

            foreach (var collar in collars)
            {
                rows.Add(new CollarRecoveryResource(
                    user?.Username ?? $"user#{sub.UserId}",
                    user?.Email ?? "-",
                    collar.DeviceId,
                    sub.SuspendedAt,
                    sub.Status,
                    collar.LifecycleStatus.ToString()));
            }
        }

        return Ok(rows);
    }
}
