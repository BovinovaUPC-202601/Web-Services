using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Commands;

namespace VacApp_Bovinova_Platform.CampaignManagement.Domain.Services;

public interface ICampaignCommandService
{
    Task<Campaign?> Handle(CreateCampaignCommand command);
    Task<Campaign?> Handle(UpdateCampaignCommand command);
    Task<bool> Handle(DeleteCampaignCommand command);
}