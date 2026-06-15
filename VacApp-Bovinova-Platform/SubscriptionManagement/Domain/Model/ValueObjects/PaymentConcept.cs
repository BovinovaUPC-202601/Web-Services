namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

/// <summary>
/// What a payment is for: Plus base (S/149/month) or an additional collar
/// slot (S/25/month).
/// </summary>
public enum PaymentConcept
{
    PlusMonthly,
    AdditionalCollar
}
