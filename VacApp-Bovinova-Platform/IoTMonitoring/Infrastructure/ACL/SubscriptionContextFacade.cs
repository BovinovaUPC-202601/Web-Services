using VacApp_Bovinova_Platform.IoTMonitoring.Application.ACL;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Infrastructure.ACL;

public class SubscriptionContextFacade(
    ISubscriptionQueryService subscriptionQueryService,
    ISubscriptionRepository subscriptionRepository)
    : ISubscriptionContextFacade
{
    public async Task<int> GetCollarAllowanceAsync(int userId)
        => await subscriptionQueryService.GetCollarAllowanceAsync(userId);

    public async Task<IEnumerable<InactiveSubscriptionInfo>> GetInactiveSubscriptionsAsync()
    {
        var inactive = await subscriptionRepository.FindInactiveAsync();
        return inactive.Select(s => new InactiveSubscriptionInfo(
            s.UserId, s.Status.ToString(), s.SuspendedAt));
    }
}
