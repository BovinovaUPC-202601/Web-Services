namespace VacApp_Bovinova_Platform.IAM.Domain.Model.Commands
{
    public record ChangeSubscriptionCommand(
        int Id,
        string SubscriptionPlan
    );
}
