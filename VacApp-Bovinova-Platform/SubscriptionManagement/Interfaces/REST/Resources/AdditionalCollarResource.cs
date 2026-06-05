namespace VacApp_Bovinova_Platform.SubscriptionManagement.Interfaces.REST.Resources;

public record AdditionalCollarResource(int Id, string Status, decimal MonthlyAmount, DateTime RequestedAt);
