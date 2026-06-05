using VacApp_Bovinova_Platform.IAM.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IAM.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.IAM.Interfaces.REST.Transform;

public static class ChangeSubscriptionCommandFromResourceAssembler
{
    public static ChangeSubscriptionCommand ToCommandFromResource(UpdateSubscriptionResource resource, int id)
    {
        return new ChangeSubscriptionCommand(id, resource.SubscriptionPlan);
    }
}
