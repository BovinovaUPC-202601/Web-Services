using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Transform;

public static class CreateCampaignCommandFromResourceAssembler
{
    public static CreateCampaignCommand ToCommandFromResource(CreateCampaignResource resource, int userId)
    {
        return new CreateCampaignCommand(
                   resource.Name,
                   resource.Description,
                   resource.StartDate,
                   resource.EndDate,
                   userId,
                   resource.StableIds
                   );

    }
}