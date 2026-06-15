using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;
using VacApp_Bovinova_Platform.SubscriptionManagement.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/subscriptions")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Payments")]
public class PaymentController(
    IPaymentCommandService paymentCommandService,
    IPaymentRepository paymentRepository)
    : ControllerBase
{
    /// <summary>Starts checkout for the Plus plan. Returns the URL to redirect to.</summary>
    [HttpPost("plus/checkout")]
    [SwaggerResponse(StatusCodes.Status200OK, "Checkout session created", typeof(CheckoutResource))]
    public async Task<IActionResult> CheckoutPlus()
        => await CreateCheckout(PaymentConcept.PlusMonthly);

    /// <summary>Starts checkout for an additional collar slot (S/25/month).</summary>
    [HttpPost("additional-collars/checkout")]
    [SwaggerResponse(StatusCodes.Status200OK, "Checkout session created", typeof(CheckoutResource))]
    public async Task<IActionResult> CheckoutAdditionalCollar()
        => await CreateCheckout(PaymentConcept.AdditionalCollar);

    private async Task<IActionResult> CreateCheckout(PaymentConcept concept)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        var session = await paymentCommandService.Handle(
            new CreateCheckoutCommand(user.Id, user.Email, concept));
        return Ok(new CheckoutResource(session.CheckoutUrl));
    }

    /// <summary>The authenticated user's billing history, newest first.</summary>
    [HttpGet("payments")]
    [SwaggerResponse(StatusCodes.Status200OK, "Billing history", typeof(IEnumerable<PaymentResource>))]
    public async Task<IActionResult> GetPayments()
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        var payments = await paymentRepository.FindByUserIdAsync(user.Id);
        return Ok(payments.Select(p => new PaymentResource(
            p.Id, p.Concept.ToString(), p.Amount, p.Currency,
            p.Status.ToString(), p.PaidAt, p.CreatedDate)));
    }

    /// <summary>
    /// Confirms a checkout after the simulated card form. Owner-only; idempotent.
    /// </summary>
    [HttpPost("checkout/{sessionRef}/confirm")]
    [SwaggerResponse(StatusCodes.Status200OK, "Payment confirmed")]
    public async Task<IActionResult> ConfirmCheckout([FromRoute] string sessionRef)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        await paymentCommandService.ConfirmCheckoutAsync(sessionRef, user.Id);
        return Ok(new { status = "confirmed" });
    }
}
