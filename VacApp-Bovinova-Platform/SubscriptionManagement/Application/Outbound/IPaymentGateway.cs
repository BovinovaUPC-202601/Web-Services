using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;

/// <summary>
/// Outbound port to the payment provider. The Application layer depends on this
/// abstraction only; the concrete adapter (mock) lives in Infrastructure, so a real
/// provider could be plugged in later without touching the domain (hexagonal).
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Opens a checkout for a concept and returns where to redirect the user plus the
    /// provider reference (session id) to persist on the pending payment.
    /// </summary>
    Task<CheckoutSession> CreateSubscriptionCheckoutAsync(CheckoutRequest request);
}

/// <summary>Inputs needed to open a checkout session.</summary>
public record CheckoutRequest(
    int UserId,
    string UserEmail,
    PaymentConcept Concept,
    decimal Amount,
    string IdempotencyKey);

/// <summary>Result of opening a checkout session.</summary>
public record CheckoutSession(string CheckoutUrl, string SessionRef);
