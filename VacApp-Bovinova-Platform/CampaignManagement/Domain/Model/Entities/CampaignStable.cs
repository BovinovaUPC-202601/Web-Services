namespace VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Entities;

public class CampaignStable
{
    public int CampaignId { get; private set; }
    public int StableId { get; private set; }

    protected CampaignStable() { }

    public CampaignStable(int stableId)
    {
        StableId = stableId;
    }
}
