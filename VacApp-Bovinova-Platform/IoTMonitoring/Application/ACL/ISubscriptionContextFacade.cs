namespace VacApp_Bovinova_Platform.IoTMonitoring.Application.ACL;

/// <summary>Subscription info IoTMonitoring needs for the collar recovery report.</summary>
public record InactiveSubscriptionInfo(int UserId, string Status, DateTime? SuspendedAt);

/// <summary>
/// Anti-corruption layer: lets IoTMonitoring ask SubscriptionManagement how many
/// collars a user is allowed (3 included + approved additional requests) and which
/// subscriptions are inactive, without depending on subscription internals.
/// </summary>
public interface ISubscriptionContextFacade
{
    Task<int> GetCollarAllowanceAsync(int userId);

    Task<IEnumerable<InactiveSubscriptionInfo>> GetInactiveSubscriptionsAsync();
}
