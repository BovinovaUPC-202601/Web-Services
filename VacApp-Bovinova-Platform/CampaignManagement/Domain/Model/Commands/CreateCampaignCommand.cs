namespace VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Commands;

public record CreateCampaignCommand(
    string Name,
    string Description,
    DateOnly StartDate,
    DateOnly EndDate,
    int UserId,
    List<int> StableIds,
    List<int> BovineIds
    );