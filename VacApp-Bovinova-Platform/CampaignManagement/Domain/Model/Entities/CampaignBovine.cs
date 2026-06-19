namespace VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Entities;

public class CampaignBovine
{
    public int CampaignId { get; private set; }
    public int BovineId { get; private set; }

    protected CampaignBovine() { }

    public CampaignBovine(int bovineId)
    {
        BovineId = bovineId;
    }
}
