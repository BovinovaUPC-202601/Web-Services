namespace VacApp_Bovinova_Platform.SubscriptionManagement.Interfaces.REST.Resources;

/// <summary>A single payment in the user's billing history.</summary>
public record PaymentResource(
    int Id,
    string Concept,
    decimal Amount,
    string Currency,
    string Status,
    DateTime? PaidAt,
    DateTimeOffset? CreatedAt);
