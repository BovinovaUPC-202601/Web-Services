using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Entities;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.RanchManagement.Application.Internal.CommandServices;

public class BovineBreedCommandService(
    IBovineBreedRepository bovineBreedRepository,
    IUnitOfWork unitOfWork)
    : IBovineBreedCommandService
{
    public async Task<BovineBreed?> Handle(CreateBovineBreedCommand command)
    {
        var breed = new BovineBreed(
            command.Name,
            command.MinTemperature,
            command.MaxTemperature,
            command.MinHeartRate,
            command.MaxHeartRate,
            command.UserId
        );

        try
        {
            await bovineBreedRepository.AddAsync(breed);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception)
        {
            return null;
        }

        return breed;
    }

    public async Task<BovineBreed?> Handle(UpdateBovineBreedCommand command)
    {
        var breed = await bovineBreedRepository.FindByIdAsync(command.Id);
        if (breed is null) return null;

        breed.Update(command.Name, command.MinTemperature, command.MaxTemperature, command.MinHeartRate, command.MaxHeartRate);

        try
        {
            bovineBreedRepository.Update(breed);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception)
        {
            return null;
        }

        return breed;
    }

    public async Task<BovineBreed?> Handle(DeleteBovineBreedCommand command)
    {
        var breed = await bovineBreedRepository.FindByIdAsync(command.Id);
        if (breed is null) return null;

        try
        {
            bovineBreedRepository.Remove(breed);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception)
        {
            return null;
        }

        return breed;
    }
}
