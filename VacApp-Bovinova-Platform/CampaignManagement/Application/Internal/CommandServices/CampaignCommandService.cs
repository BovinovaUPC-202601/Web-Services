using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.CampaignManagement.Application.Internal.CommandServices;

public class CampaignCommandService(ICampaignRepository campaignRepository, IUnitOfWork unitOfWork)
: ICampaignCommandService
{
    public async Task<Campaign?> Handle(CreateCampaignCommand command)
    {
        var campaign = new Campaign(command);
        try
        {
            await campaignRepository.AddAsync(campaign);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception)
        {
            return null;
        }
        return campaign;
    }

    public async Task<Campaign?> Handle(UpdateCampaignCommand command)
    {
        var campaign = await campaignRepository.FindByIdAsync(command.Id);
        if (campaign is null) return null;

        // Evita guardar campañas con rangos de fechas inconsistentes.
        if (command.EndDate < command.StartDate)
            throw new ArgumentException("EndDate no puede ser anterior a StartDate.");

        campaign.Update(command);

        try
        {
            campaignRepository.Update(campaign);
            await unitOfWork.CompleteAsync();
            return campaign;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> Handle(DeleteCampaignCommand command)
    {
        var campaign = await campaignRepository.FindByIdAsync(command.id);
        if (campaign is null) return false;

        try
        {
            campaignRepository.Remove(campaign);
            await unitOfWork.CompleteAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}