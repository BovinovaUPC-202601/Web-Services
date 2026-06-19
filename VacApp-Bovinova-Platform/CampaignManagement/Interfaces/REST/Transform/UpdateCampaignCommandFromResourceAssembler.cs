using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Transform;

public static class UpdateCampaignCommandFromResourceAssembler
{
    public static UpdateCampaignCommand ToCommandFromResource(int id, UpdateCampaignResource resource, int effectiveUserId)
    {
        return new UpdateCampaignCommand(
            id,
            resource.Name,
            resource.Description,
            resource.StartDate,
            resource.EndDate,
            effectiveUserId,
            resource.StableIds ?? [],
            resource.BovineIds ?? []
        );
    }
}
