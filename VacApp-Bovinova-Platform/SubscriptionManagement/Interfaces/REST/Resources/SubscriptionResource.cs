namespace VacApp_Bovinova_Platform.SubscriptionManagement.Interfaces.REST.Resources;

public record SubscriptionResource(
    string Plan,
    string Status,
    DateTime? StartDate,
    DateTime? NextRenewal,
    int IncludedCollars,
    int AdditionalCollars,
    decimal MonthlyCost);
