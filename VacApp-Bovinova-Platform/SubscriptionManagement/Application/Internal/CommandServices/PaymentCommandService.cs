using Microsoft.Extensions.Logging;
using VacApp_Bovinova_Platform.IAM.Domain.Model;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Application.ACL;
using VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Application.Internal.CommandServices;

/// <summary>
/// Drives payments through the gateway and reconciles the subscription on
/// confirmation. The subscription is only activated here (on confirm), never from
/// the synchronous checkout request, so money and access stay consistent.
/// </summary>
public class PaymentCommandService(
    IPaymentRepository paymentRepository,
    ISubscriptionRepository subscriptionRepository,
    IAdditionalCollarRequestRepository additionalCollarRepository,
    IUserRepository userRepository,
    ICollarLifecycleFacade collarLifecycle,
    IPaymentGateway gateway,
    IEmailSender emailSender,
    ILogger<PaymentCommandService> logger,
    IUnitOfWork unitOfWork)
    : IPaymentCommandService
{
    public async Task<CheckoutSession> Handle(CreateCheckoutCommand command)
    {
        var amount = command.Concept == PaymentConcept.PlusMonthly
            ? SubscriptionPricing.PlusBaseMonthly
            : SubscriptionPricing.AdditionalCollarMonthly;

        var idempotencyKey = Guid.NewGuid().ToString();
        var payment = new Payment(command.UserId, command.Concept, amount, idempotencyKey);
        await paymentRepository.AddAsync(payment);

        var session = await gateway.CreateSubscriptionCheckoutAsync(new CheckoutRequest(
            command.UserId, command.UserEmail, command.Concept, amount, idempotencyKey));

        // Persist the gateway session ref so the confirmation can resolve this payment.
        payment.LinkProviderRef(session.SessionRef);
        await unitOfWork.CompleteAsync();
        return session;
    }

    public async Task ConfirmCheckoutAsync(string sessionRef, int userId)
    {
        var payment = await paymentRepository.FindByProviderRefAsync(sessionRef);
        if (payment is null) return;                          // unknown session
        if (payment.UserId != userId) return;                 // not the owner's session
        if (payment.Status != PaymentStatus.Pending) return;  // already confirmed (idempotent)

        payment.MarkPaid(sessionRef);

        var user = await userRepository.FindByIdAsync(userId);

        if (payment.Concept == PaymentConcept.PlusMonthly)
        {
            var subscription = await subscriptionRepository.FindByUserIdAsync(userId);
            if (subscription is null)
            {
                subscription = new Subscription(userId);
                await subscriptionRepository.AddAsync(subscription);
            }

            subscription.ActivatePlus();
            user?.ChangeSubscription(SubscriptionPlans.Plus);

            await unitOfWork.CompleteAsync();
            await collarLifecycle.ReactivateUserCollarsAsync(userId);
        }
        else // AdditionalCollar: payment grants the slot directly (no admin step).
        {
            var request = new AdditionalCollarRequest(userId, SubscriptionPricing.AdditionalCollarMonthly);
            request.Approve();
            request.Deliver();
            await additionalCollarRepository.AddAsync(request);
            await unitOfWork.CompleteAsync();
        }

        await SendReceiptAsync(payment, user);
    }

    /// <summary>
    /// Emails the purchase receipt. Best-effort: a mail failure is logged but never
    /// propagated, so it cannot roll back an already-confirmed payment. Reuses the
    /// gateway session ref as the idempotency key to avoid duplicate sends on retries.
    /// </summary>
    private async Task SendReceiptAsync(Payment payment, User? user)
    {
        if (user is null || string.IsNullOrWhiteSpace(user.Email)) return;

        try
        {
            var (subject, html) = ReceiptEmail.Build(
                payment.Concept, payment.Amount, payment.Currency,
                payment.PaidAt ?? DateTime.UtcNow);

            await emailSender.SendAsync(user.Email, subject, html, payment.ProviderRef);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Receipt email failed for payment {PaymentId} (payment stands).", payment.Id);
        }
    }
}
