using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Repositories;

public interface ISubscriptionRepository : IBaseRepository<Subscription>
{
    Task<Subscription?> FindByUserIdAsync(int userId);

    /// <summary>Subscriptions that are suspended or cancelled (for the recovery report).</summary>
    Task<IEnumerable<Subscription>> FindInactiveAsync();

    /// <summary>Active Plus subscriptions with a renewal date set (for pre-renewal reminders).</summary>
    Task<IEnumerable<Subscription>> FindActivePlusWithRenewalAsync();
}
