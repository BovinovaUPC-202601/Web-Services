namespace VacApp_Bovinova_Platform.IAM.Interfaces.REST.Resources.UserResources;

public record UserInfoResource(
   int id,
   string name,
   int totalAnimals,
   int totalCampaigns,
   int totalStaff,
   int totalProducts,
   int totalStables,
   CampaignInfoResource[] nextCampaigns
);

public record CampaignInfoResource(
    int id,
    string name,
    DateOnly startDate,
    DateOnly endDate
);