using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;
using VacApp_Bovinova_Platform.SubscriptionManagement.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.SubscriptionManagement.Interfaces.REST.Transform;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/subscriptions")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Subscriptions")]
public class SubscriptionController(
    ISubscriptionCommandService commandService,
    ISubscriptionQueryService queryService,
    ICollarQueryService collarQueryService,
    IStaffAccessService staffAccessService)
    : ControllerBase
{
    /// <summary>
    /// Subscription management is owner-only: staff of any level get 403.
    /// Note this intentionally does NOT use effectiveUserId — staff must never
    /// manage (nor read) the owner's subscription. Returns null when allowed.
    /// </summary>
    private async Task<IActionResult?> ForbidStaffAsync(User user)
    {
        if (await staffAccessService.IsStaffAsync(user))
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "Only the ranch owner can manage the subscription." });
        return null;
    }

    /// <summary>Lists the available plans with prices, features and included collars (TS020).</summary>
    [AllowAnonymous]
    [HttpGet("plans")]
    [SwaggerResponse(StatusCodes.Status200OK, "Available plans", typeof(IEnumerable<PlanResource>))]
    public IActionResult GetPlans()
    {
        var plans = new[]
        {
            new PlanResource("Free", 0m, 0, 0m,
                new[] { "Gestión básica del ganado" }),
            new PlanResource("Plus", SubscriptionPricing.PlusBaseMonthly,
                SubscriptionPricing.IncludedCollars, SubscriptionPricing.AdditionalCollarMonthly,
                new[] { "IA", "IoT", "Alertas biométricas", "3 collares incluidos" })
        };
        return Ok(plans);
    }

    /// <summary>Returns the authenticated user's current subscription and monthly cost (TS020).</summary>
    [HttpGet("current")]
    [SwaggerResponse(StatusCodes.Status200OK, "Current subscription", typeof(SubscriptionResource))]
    public async Task<IActionResult> GetCurrent()
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        if (await ForbidStaffAsync(user) is { } forbidden) return forbidden;

        // Self-heal any drift between the IAM flag and the subscription aggregate
        // (the frontend hits this on every layout mount), so [RequiresPlus] stays correct.
        await commandService.SyncIamPlanAsync(user.Id);

        var subscription = await queryService.Handle(new GetSubscriptionByUserIdQuery(user.Id));
        if (subscription is null)
            return Ok(new SubscriptionResource("Free", "Active", null, null, 0, 0, 0m));

        var activeCollars = await collarQueryService.GetActiveCountAsync(user.Id);
        return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription, activeCollars));
    }

    /// <summary>Activates the Plus subscription (simulated payment) (TS021).</summary>
    [HttpPost("plus/activate")]
    [SwaggerResponse(StatusCodes.Status200OK, "Plus activated", typeof(SubscriptionResource))]
    public async Task<IActionResult> ActivatePlus()
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        if (await ForbidStaffAsync(user) is { } forbidden) return forbidden;

        var subscription = await commandService.Handle(new ActivatePlusCommand(user.Id));
        var activeCollars = await collarQueryService.GetActiveCountAsync(user.Id);
        return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription, activeCollars));
    }

    /// <summary>Requests an additional collar slot (S/25/month, simulated auto-approval) (TS023).</summary>
    [HttpPost("additional-collars")]
    [SwaggerResponse(StatusCodes.Status200OK, "Additional collar requested", typeof(AdditionalCollarResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Active Plus subscription required")]
    public async Task<IActionResult> RequestAdditionalCollar()
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        if (await ForbidStaffAsync(user) is { } forbidden) return forbidden;

        try
        {
            var request = await commandService.Handle(new RequestAdditionalCollarCommand(user.Id));
            return Ok(new AdditionalCollarResource(
                request.Id, request.Status.ToString(), request.MonthlyAmount, request.RequestedAt));
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>Cancels the authenticated user's own subscription.</summary>
    [HttpPost("cancel")]
    [SwaggerResponse(StatusCodes.Status200OK, "Subscription cancelled")]
    public async Task<IActionResult> CancelOwn()
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        if (await ForbidStaffAsync(user) is { } forbidden) return forbidden;

        var subscription = await commandService.Handle(new CancelSubscriptionCommand(user.Id));
        if (subscription is null) return NotFound("Subscription not found.");
        return Ok(new { message = "Subscription cancelled", status = subscription.Status.ToString() });
    }

    /// <summary>Admin: suspends a user's subscription for non-payment (simulated) (TS019).</summary>
    [RequiresAdmin]
    [HttpPost("{userId:int}/suspend")]
    [SwaggerResponse(StatusCodes.Status200OK, "Subscription suspended")]
    public async Task<IActionResult> Suspend([FromRoute] int userId)
    {
        var subscription = await commandService.Handle(new SuspendSubscriptionCommand(userId));
        if (subscription is null) return NotFound("Subscription not found.");
        return Ok(new { message = "Subscription suspended", status = subscription.Status.ToString() });
    }

    /// <summary>Admin: approves a pending additional-collar request (TS023).</summary>
    [RequiresAdmin]
    [HttpPut("additional-collars/{requestId:int}/approve")]
    [SwaggerResponse(StatusCodes.Status200OK, "Request approved", typeof(AdditionalCollarResource))]
    public async Task<IActionResult> ApproveAdditionalCollar([FromRoute] int requestId)
    {
        var request = await commandService.Handle(new ApproveAdditionalCollarCommand(requestId));
        if (request is null) return NotFound("Request not found.");
        return Ok(new AdditionalCollarResource(
            request.Id, request.Status.ToString(), request.MonthlyAmount, request.RequestedAt));
    }

    /// <summary>Admin: marks an approved additional collar as delivered/activated (TS023).</summary>
    [RequiresAdmin]
    [HttpPut("additional-collars/{requestId:int}/deliver")]
    [SwaggerResponse(StatusCodes.Status200OK, "Request delivered", typeof(AdditionalCollarResource))]
    public async Task<IActionResult> DeliverAdditionalCollar([FromRoute] int requestId)
    {
        var request = await commandService.Handle(new DeliverAdditionalCollarCommand(requestId));
        if (request is null) return NotFound("Request not found.");
        return Ok(new AdditionalCollarResource(
            request.Id, request.Status.ToString(), request.MonthlyAmount, request.RequestedAt));
    }
}
