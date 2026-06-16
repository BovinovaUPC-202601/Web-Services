using Microsoft.Extensions.Logging;
using VacApp_Bovinova_Platform.IAM.Domain.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Application.Internal.CommandServices;

/// <summary>
/// Suspends active Plus subscriptions whose renewal date has passed without a new
/// payment (no grace period). Reuses the existing suspend command — which downgrades the
/// user to Free and cascades the collar shutdown — then emails the suspension notice.
/// </summary>
public class SubscriptionSuspensionService(
    ISubscriptionRepository subscriptionRepository,
    IAdditionalCollarRequestRepository additionalCollarRepository,
    IUserRepository userRepository,
    ISubscriptionCommandService commandService,
    IEmailSender emailSender,
    ILogger<SubscriptionSuspensionService> logger)
    : ISubscriptionSuspensionService
{
    public async Task<int> SuspendOverdueAsync()
    {
        var today = DateTime.UtcNow.Date;
        var subscriptions = await subscriptionRepository.FindActivePlusWithRenewalAsync();
        var suspended = 0;

        foreach (var subscription in subscriptions)
        {
            // Overdue means the cycle already ended (suspend on the day it lapses).
            if (subscription.NextRenewal!.Value.Date >= today) continue;

            // Capture details before suspending (the command downgrades the user to Free).
            var user = await userRepository.FindByIdAsync(subscription.UserId);
            var extraCollars = await additionalCollarRepository.CountActiveByUserIdAsync(subscription.UserId);
            var total = SubscriptionPricing.PlusBaseMonthly
                        + SubscriptionPricing.AdditionalCollarMonthly * extraCollars;
            var renewal = subscription.NextRenewal.Value;

            var result = await commandService.Handle(new SuspendSubscriptionCommand(subscription.UserId));
            if (result is null) continue;
            suspended++;

            if (user is null || string.IsNullOrWhiteSpace(user.Email)) continue;

            try
            {
                var (subject, html) = SuspensionEmail.Build(total, "PEN", renewal, extraCollars);
                var key = $"suspended-{subscription.Id}-{renewal:yyyyMMdd}";
                await emailSender.SendAsync(user.Email, subject, html, key);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Suspension email failed for subscription {Id} (subscription still suspended).", subscription.Id);
            }
        }

        logger.LogInformation("Subscriptions suspended for non-payment: {Count}", suspended);
        return suspended;
    }
}
