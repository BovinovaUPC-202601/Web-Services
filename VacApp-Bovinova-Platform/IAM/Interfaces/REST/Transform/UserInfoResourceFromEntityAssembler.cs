using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Interfaces.REST.Resources.UserResources;

namespace VacApp_Bovinova_Platform.IAM.Interfaces.REST.Transform;

public static class UserInfoResourceFromEntityAssembler
{
    public static UserInfoResource ToResourceFromEntity(
        User user,
        bool isStaff,
        int effectiveUserId,
        string accessLevel,
        bool canEdit,
        bool canManageStaff,
        bool canManageSubscription,
        string subscriptionPlan,
        int totalBovines,
        int totalCampaigns,
        int totalStaff,
        int totalProducts,
        int totalStables,
        CampaignInfoResource[] nextCampaigns)
    {
        return new UserInfoResource(
            user.Id,
            user.Username,
            user.Email,
            subscriptionPlan,
            isStaff,
            effectiveUserId,
            accessLevel,
            true,
            canEdit,
            canManageStaff,
            canManageSubscription,
            totalBovines,
            totalCampaigns,
            totalStaff,
            totalProducts,
            totalStables,
            nextCampaigns
        );
    }
}
