using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;

namespace VacApp_Bovinova_Platform.RanchManagement.Infrastructure.Reminders;

public class ProductExpiryBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ProductExpiryBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IProductExpiryNotificationService>();
                await service.SendExpiryNotificationsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Product expiry sweep failed; will retry next cycle.");
            }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
}
