namespace VacApp_Bovinova_Platform.SubscriptionManagement.Application.ACL;

/// <summary>
/// Anti-corruption layer: lets SubscriptionManagement cascade lifecycle changes to a
/// user's collars (suspend on non-payment, reactivate on renewal) without depending
/// on IoTMonitoring internals.
/// </summary>
public interface ICollarLifecycleFacade
{
    Task SuspendUserCollarsAsync(int userId);
    Task ReactivateUserCollarsAsync(int userId);
}
