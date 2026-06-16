using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Transform;

public static class CampaignResourceFromEntityAssembler
{
    public static CampaignResource ToResourceFromEntity(Campaign campaign, List<string>? stableNames = null)
    {
        return new CampaignResource(
               campaign.Id,
               campaign.Name,
               campaign.Description,
               campaign.StartDate,
               campaign.EndDate,
               campaign.UserId,
               campaign.CampaignStables.Select(cs => cs.StableId).ToList(),
               stableNames ?? new List<string>());
    }
}