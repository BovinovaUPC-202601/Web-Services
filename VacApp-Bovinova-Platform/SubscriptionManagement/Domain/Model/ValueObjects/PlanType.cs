namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

/// <summary>
/// Subscription plans. Free has no IA/IoT/collars; Plus unlocks them (TP EP009).
/// </summary>
public enum PlanType
{
    Free,
    Plus
}
