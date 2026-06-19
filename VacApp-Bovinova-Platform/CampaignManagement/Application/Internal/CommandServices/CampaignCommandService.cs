using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Services;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.CampaignManagement.Application.Internal.CommandServices;

public class CampaignCommandService(
    ICampaignRepository campaignRepository,
    IUnitOfWork unitOfWork,
    IStableRepository stableRepository,
    IBovineRepository bovineRepository)
: ICampaignCommandService
{
    public async Task<Campaign?> Handle(CreateCampaignCommand command)
    {
        foreach (var stableId in command.StableIds)
        {
            var stable = await stableRepository.FindByIdAsync(stableId);
            if (stable is null)
                throw new NotFoundException($"Establo con ID '{stableId}' no encontrado.");
        }

        foreach (var bovineId in command.BovineIds)
        {
            var bovine = await bovineRepository.FindByIdAsync(bovineId);
            if (bovine is null)
                throw new NotFoundException($"Bovino con ID '{bovineId}' no encontrado.");
        }

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

        foreach (var stableId in command.StableIds)
        {
            var stable = await stableRepository.FindByIdAsync(stableId);
            if (stable is null)
                throw new NotFoundException($"Establo con ID '{stableId}' no encontrado.");
        }

        foreach (var bovineId in command.BovineIds)
        {
            var bovine = await bovineRepository.FindByIdAsync(bovineId);
            if (bovine is null)
                throw new NotFoundException($"Bovino con ID '{bovineId}' no encontrado.");
        }

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