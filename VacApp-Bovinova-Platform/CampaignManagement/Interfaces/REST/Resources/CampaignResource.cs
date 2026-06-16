namespace VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Resources;

public record CampaignResource(
    int Id,
    string Name,
    string Description,
    DateOnly StartDate,
    DateOnly EndDate,
    int UserId,
    List<int> StableIds,
    List<string> StableNames);