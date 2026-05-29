namespace VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Resources;

public record UpdateCampaignResource(
    string Name,
    string Description,
    DateOnly StartDate,
    DateOnly EndDate
);
