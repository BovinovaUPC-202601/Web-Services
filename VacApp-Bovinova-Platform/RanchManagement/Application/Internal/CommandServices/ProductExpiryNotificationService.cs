using Microsoft.Extensions.Logging;
using VacApp_Bovinova_Platform.IAM.Domain.Repositories;
using VacApp_Bovinova_Platform.RanchManagement.Application.Outbound;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;

namespace VacApp_Bovinova_Platform.RanchManagement.Application.Internal.CommandServices;

public class ProductExpiryNotificationService(
    IProductRepository productRepository,
    IUserRepository userRepository,
    IEmailSender emailSender,
    ILogger<ProductExpiryNotificationService> logger,
    IUnitOfWork unitOfWork)
    : IProductExpiryNotificationService
{
    private static readonly int NotificationWindowDays = 7;

    public async Task<int> SendExpiryNotificationsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var until = today.AddDays(NotificationWindowDays);

        var products = await productRepository.FindByExpirationDateWindowAsync(today, until);
        if (!products.Any())
        {
            logger.LogInformation("No expiring products found in the next {Days} days.", NotificationWindowDays);
            return 0;
        }

        var byUser = products.GroupBy(p => p.UserId);
        var sent = 0;

        foreach (var group in byUser)
        {
            var user = await userRepository.FindByIdAsync(group.Key);
            if (user is null || string.IsNullOrWhiteSpace(user.Email))
            {
                logger.LogWarning("User {UserId} not found or has no email; skipping expiry notification.", group.Key);
                continue;
            }

            var productList = group.ToList();

            try
            {
                var (subject, html) = ProductExpiryEmail.Build(productList);
                var key = $"product-expiry-{group.Key}-{today:yyyyMMdd}";
                await emailSender.SendAsync(user.Email, subject, html, key);

                foreach (var product in productList)
                    product.MarkExpiryNotificationSent();

                sent++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Expiry notification email failed for user {UserId} (will retry next run).", group.Key);
            }
        }

        if (sent > 0) await unitOfWork.CompleteAsync();
        logger.LogInformation("Product expiry notifications sent: {Count}", sent);
        return sent;
    }
}
