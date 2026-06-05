namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

/// <summary>
/// Lifecycle states of a subscription (TP US032).
/// </summary>
public enum SubscriptionStatus
{
    Active,
    Expired,
    Suspended,
    Cancelled
}
