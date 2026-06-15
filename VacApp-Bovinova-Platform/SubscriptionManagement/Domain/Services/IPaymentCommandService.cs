using VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Commands;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;

public interface IPaymentCommandService
{
    /// <summary>Creates a pending payment + checkout session; returns where to redirect.</summary>
    Task<CheckoutSession> Handle(CreateCheckoutCommand command);

    /// <summary>
    /// Confirms a checkout by its session ref and drives the subscription change.
    /// Only the owning user may confirm. Idempotent — re-confirming does nothing.
    /// </summary>
    Task ConfirmCheckoutAsync(string sessionRef, int userId);
}
