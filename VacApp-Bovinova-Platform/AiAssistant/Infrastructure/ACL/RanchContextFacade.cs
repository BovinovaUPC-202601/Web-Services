using System.Text;
using VacApp_Bovinova_Platform.AIAssistant.Application.ACL;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Services;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;

namespace VacApp_Bovinova_Platform.AIAssistant.Infrastructure.ACL;

public class RanchContextFacade(
    IBovineQueryService bovineQueryService,
    IStableQueryService stableQueryService,
    ICampaignQueryService campaignQueryService) : IRanchContextFacade
{
    public async Task<string> GetGeneralRanchContextAsync(int userId)
    {
        var bovines = (await bovineQueryService.Handle(new GetAllBovinesQuery(userId))).ToList();
        var stables = (await stableQueryService.Handle(new GetAllStablesQuery(userId))).ToList();
        var campaigns = (await campaignQueryService.Handle(new GetAllCampaignsQuery(userId))).ToList();

        var context = new StringBuilder();
        context.AppendLine($"Authenticated ranch user id: {userId}");
        context.AppendLine($"Registered bovines: {bovines.Count}");
        foreach (var bovine in bovines)
        {
            context.AppendLine(
                $"- Bovine #{bovine.Id}: {bovine.Name}, {bovine.Gender}, {bovine.Breed}, born {bovine.BirthDate:yyyy-MM-dd}, stable #{bovine.StableId}");
        }

        context.AppendLine($"Registered stables: {stables.Count}");
        foreach (var stable in stables)
        {
            context.AppendLine($"- Stable #{stable.Id}: {stable.Name}, capacity {stable.Limit}");
        }

        context.AppendLine($"Registered campaigns: {campaigns.Count}");
        foreach (var campaign in campaigns)
        {
            var stablesText = campaign.CampaignStables.Count > 0
                ? string.Join(", ", campaign.CampaignStables.Select(cs => $"#{cs.StableId}"))
                : "none";
            var bovinesText = campaign.CampaignBovines.Count > 0
                ? string.Join(", ", campaign.CampaignBovines.Select(cb => $"#{cb.BovineId}"))
                : "none";
            context.AppendLine(
                $"- Campaign #{campaign.Id}: {campaign.Name}, {campaign.Description}, from {campaign.StartDate:yyyy-MM-dd} to {campaign.EndDate:yyyy-MM-dd}; stables: {stablesText}; bovines: {bovinesText}");
        }

        return context.ToString();
    }

    public async Task<string> GetBovineContextAsync(int userId, int bovineId)
    {
        var bovine = await bovineQueryService.Handle(new GetBovinesByIdQuery(bovineId));

        if (bovine is null || bovine.UserId != userId)
            throw new KeyNotFoundException($"Bovine with ID '{bovineId}' was not found for the authenticated user.");

        var context = new StringBuilder();
        context.AppendLine($"Authenticated ranch user id: {userId}");
        context.AppendLine($"Bovine id: {bovine.Id}");
        context.AppendLine($"Name: {bovine.Name}");
        context.AppendLine($"Gender: {bovine.Gender}");
        context.AppendLine($"Breed: {bovine.Breed}");
        context.AppendLine($"Birth date: {bovine.BirthDate:yyyy-MM-dd}");
        context.AppendLine($"Stable id: {bovine.StableId}");
        context.AppendLine(
            $"Normal temperature range for this bovine: {bovine.MinTemperature:0.0}-{bovine.MaxTemperature:0.0} C");
        context.AppendLine(
            $"Normal heart rate range for this bovine: {bovine.MinHeartRate}-{bovine.MaxHeartRate} bpm");

        // Campaigns covering this bovine: directly associated, or via its stable.
        var campaigns = (await campaignQueryService.Handle(new GetAllCampaignsQuery(userId))).ToList();
        var bovineCampaigns = campaigns
            .Where(c => c.CampaignBovines.Any(cb => cb.BovineId == bovine.Id)
                        || c.CampaignStables.Any(cs => cs.StableId == bovine.StableId))
            .ToList();

        if (bovineCampaigns.Count > 0)
        {
            context.AppendLine($"Campaigns covering this bovine: {bovineCampaigns.Count}");
            foreach (var campaign in bovineCampaigns)
            {
                var via = campaign.CampaignBovines.Any(cb => cb.BovineId == bovine.Id) ? "directly" : "via its stable";
                context.AppendLine(
                    $"- Campaign #{campaign.Id}: {campaign.Name}, from {campaign.StartDate:yyyy-MM-dd} to {campaign.EndDate:yyyy-MM-dd} ({via})");
            }
        }
        else
        {
            context.AppendLine("This bovine is not part of any campaign.");
        }

        return context.ToString();
    }
}
