namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

/// <summary>
/// Lifecycle of a payment against the gateway. Kept separate from
/// <see cref="SubscriptionStatus"/>: the subscription only changes once a payment
/// reaches <see cref="Paid"/> via confirmation.
/// </summary>
public enum PaymentStatus
{
    Pending,
    Paid,
    Failed,
    Refunded
}
