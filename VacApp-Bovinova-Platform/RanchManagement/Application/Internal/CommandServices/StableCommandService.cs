using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.RanchManagement.Application.Internal.CommandServices;

public class StableCommandService(
    IStableRepository stableRepository,
    IBovineRepository bovineRepository,
    IUnitOfWork unitOfWork
    ) : IStableCommandService
{
    public async Task<Stable?> Handle(CreateStableCommand command)
    {
        var stable = new Stable(command);

        try
        {
            await stableRepository.AddAsync(stable);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return stable;
    }

    public async Task<Stable?> Handle(UpdateStableCommand command)
    {
        var stable = await stableRepository.FindByIdAsync(command.Id);
        if (stable == null)
            throw new NotFoundException($"Stable con ID '{command.Id}' no encontrado.");

        if (command.Limit < stable.Limit)
        {
            var animalCount = await bovineRepository.CountBovinesByStableIdAsync(command.Id);
            if (animalCount > command.Limit)
                throw new ValidationException(
                    $"No se puede reducir la capacidad a {command.Limit} porque el establo tiene {animalCount} animales.");
        }

        stable.Update(command);

        try
        {
            stableRepository.Update(stable);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return stable;
    }

    public async Task<Stable?> Handle(DeleteStableCommand command)
    {
        // Verifies if the stable exists
        var stable = await stableRepository.FindByIdAsync(command.Id);
        if (stable == null)
            throw new NotFoundException($"Stable con ID '{command.Id}' no encontrado.");

        try
        {
            stableRepository.Remove(stable);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return stable;
    }
}