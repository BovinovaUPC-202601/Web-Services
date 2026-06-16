using System.ComponentModel.DataAnnotations;

namespace VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Resources;

public record CreateCampaignResource(
    [Required] string Name,
    [Required] string Description,
    DateOnly StartDate,
    DateOnly EndDate,
    [Required, MinLength(1)] List<int> StableIds
    );