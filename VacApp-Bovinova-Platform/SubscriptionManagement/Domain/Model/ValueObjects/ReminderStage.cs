namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

/// <summary>
/// Which pre-renewal payment reminder is being sent: one 10 days before the cycle
/// ends and another 5 days before. Each stage is sent at most once per billing cycle.
/// </summary>
public enum ReminderStage
{
    TenDays  = 10,
    FiveDays = 5
}
