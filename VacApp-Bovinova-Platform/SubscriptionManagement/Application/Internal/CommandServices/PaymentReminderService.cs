using Microsoft.Extensions.Logging;
using VacApp_Bovinova_Platform.IAM.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Application.Internal.CommandServices;

/// <summary>
/// Emails active Plus subscribers a payment reminder 10 and 5 days before their cycle
/// renews. The amount is the FULL monthly total: Plus base plus each extra collar.
/// Each stage is stamped on the subscription so a daily run never double-sends.
/// </summary>
public class PaymentReminderService(
    ISubscriptionRepository subscriptionRepository,
    IAdditionalCollarRequestRepository additionalCollarRepository,
    IUserRepository userRepository,
    IEmailSender emailSender,
    ILogger<PaymentReminderService> logger,
    IUnitOfWork unitOfWork)
    : IPaymentReminderService
{
    public async Task<int> SendDueRemindersAsync()
    {
        var today = DateTime.UtcNow.Date;
        var subscriptions = await subscriptionRepository.FindActivePlusWithRenewalAsync();
        var sent = 0;

        foreach (var subscription in subscriptions)
        {
            var daysLeft = (subscription.NextRenewal!.Value.Date - today).Days;

            // Pick the active window. The 10-day window is [6,10] so it doesn't overlap
            // the 5-day window [0,5]; each stage also guards against a repeat send.
            ReminderStage? stage = daysLeft is > 5 and <= 10 && subscription.NeedsReminder(ReminderStage.TenDays)
                ? ReminderStage.TenDays
                : daysLeft is >= 0 and <= 5 && subscription.NeedsReminder(ReminderStage.FiveDays)
                    ? ReminderStage.FiveDays
                    : null;

            if (stage is null) continue;

            var extraCollars = await additionalCollarRepository.CountActiveByUserIdAsync(subscription.UserId);
            var total = SubscriptionPricing.PlusBaseMonthly
                        + SubscriptionPricing.AdditionalCollarMonthly * extraCollars;

            var user = await userRepository.FindByIdAsync(subscription.UserId);
            if (user is null || string.IsNullOrWhiteSpace(user.Email)) continue;

            try
            {
                var (subject, html) = ReminderEmail.Build(
                    stage.Value, total, "PEN", subscription.NextRenewal.Value, extraCollars);

                // Idempotency key: one reminder per (subscription, cycle, stage).
                var key = $"reminder-{subscription.Id}-{subscription.NextRenewal.Value:yyyyMMdd}-{(int)stage.Value}";
                await emailSender.SendAsync(user.Email, subject, html, key);

                subscription.MarkReminderSent(stage.Value);
                sent++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Reminder email failed for subscription {Id} (will retry next run).", subscription.Id);
            }
        }

        if (sent > 0) await unitOfWork.CompleteAsync();
        logger.LogInformation("Payment reminders sent: {Count}", sent);
        return sent;
    }
}
