namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;

/// <summary>
/// Sends pre-renewal payment reminders. Idempotent per billing cycle: scans active
/// Plus subscriptions and emails the ones 10 or 5 days from renewal, once each.
/// </summary>
public interface IPaymentReminderService
{
    /// <summary>Sends any due reminders and returns how many emails were sent.</summary>
    Task<int> SendDueRemindersAsync();
}
