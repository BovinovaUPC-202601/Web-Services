using VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Infrastructure.PaymentGateway;

/// <summary>
/// Mock implementation of <see cref="IPaymentGateway"/>: instead of a hosted gateway,
/// checkout points the browser at a backend endpoint that auto-confirms the payment,
/// then redirects to the frontend. No keys/CLI/account needed — the only provider.
/// </summary>
public class MockPaymentGateway : IPaymentGateway
{
    private readonly string _frontBaseUrl;

    public MockPaymentGateway()
    {
        _frontBaseUrl = Environment.GetEnvironmentVariable("FRONT_BASE_URL")
                        ?? "http://localhost:5173";
    }

    public Task<CheckoutSession> CreateSubscriptionCheckoutAsync(CheckoutRequest request)
    {
        var sessionRef = $"mock_sess_{Guid.NewGuid():N}";
        // Send the browser to the app's simulated checkout page, which collects a
        // (fake) card and then calls the confirm endpoint. Mimics a hosted gateway.
        var amount = request.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var checkoutUrl = $"{_frontBaseUrl}/checkout?session={sessionRef}" +
                          $"&concept={request.Concept}&amount={amount}";
        return Task.FromResult(new CheckoutSession(checkoutUrl, sessionRef));
    }
}
