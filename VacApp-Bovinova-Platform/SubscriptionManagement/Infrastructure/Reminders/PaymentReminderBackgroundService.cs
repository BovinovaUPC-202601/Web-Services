using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Infrastructure.Reminders;

/// <summary>
/// Runs the pre-renewal payment reminder sweep once a day. Uses a scope per run because
/// the reminder service and its repositories are scoped (a hosted service is a singleton).
/// </summary>
public class PaymentReminderBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<PaymentReminderBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Small startup delay so the app finishes booting (and migrations apply) first.
        try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                // Order matters: remind first, then suspend the ones already overdue.
                var reminders = scope.ServiceProvider.GetRequiredService<IPaymentReminderService>();
                await reminders.SendDueRemindersAsync();

                var suspensions = scope.ServiceProvider.GetRequiredService<ISubscriptionSuspensionService>();
                await suspensions.SuspendOverdueAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Billing sweep (reminders/suspensions) failed; will retry next cycle.");
            }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
}
