namespace VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Commands;

public record UpdateCampaignCommand(
    int Id,
    string Name,
    string Description,
    DateOnly StartDate,
    DateOnly EndDate,
    List<int> StableIds
);
