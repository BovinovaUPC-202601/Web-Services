namespace VacApp_Bovinova_Platform.SubscriptionManagement.Interfaces.REST.Resources;

public record PlanResource(
    string Name,
    decimal MonthlyPrice,
    int IncludedCollars,
    decimal AdditionalCollarMonthly,
    IEnumerable<string> Features);
