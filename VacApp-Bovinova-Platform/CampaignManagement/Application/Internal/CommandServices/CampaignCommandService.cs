using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.CampaignManagement.Application.Internal.CommandServices;

public class CampaignCommandService(ICampaignRepository campaignRepository, IUnitOfWork unitOfWork)
: ICampaignCommandService
{
    public async Task<Campaign?> Handle(CreateCampaignCommand command)
    {
        var campaign = new Campaign(command);

        await campaignRepository.AddAsync(campaign);
        await unitOfWork.CompleteAsync();

        return campaign;
    }

    public async Task<Campaign?> Handle(UpdateCampaignCommand command)
    {
        var campaign = await campaignRepository.FindByIdAsync(command.Id);
        if (campaign is null)
            throw new NotFoundException("Campaña no encontrada.");

        campaign.Update(command);

        campaignRepository.Update(campaign);
        await unitOfWork.CompleteAsync();
        return campaign;
    }

    public async Task<bool> Handle(DeleteCampaignCommand command)
    {
        var campaign = await campaignRepository.FindByIdAsync(command.id);
        if (campaign is null)
            throw new NotFoundException("Campaña no encontrada.");

        campaignRepository.Remove(campaign);
        await unitOfWork.CompleteAsync();
        return true;
    }
}