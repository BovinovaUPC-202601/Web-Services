namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;

/// <summary>
/// Suspends Plus subscriptions whose billing cycle ended without payment. Idempotent:
/// once suspended a subscription is no longer Active, so a later run skips it.
/// </summary>
public interface ISubscriptionSuspensionService
{
    /// <summary>Suspends all overdue-unpaid subscriptions and returns how many were suspended.</summary>
    Task<int> SuspendOverdueAsync();
}
