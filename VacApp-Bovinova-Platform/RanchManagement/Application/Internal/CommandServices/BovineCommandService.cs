using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Application.OutboundServices;
using VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.RanchManagement.Application.Internal.CommandServices;

public class BovineCommandService(
    IBovineRepository bovineRepository,
    IStableRepository stableRepository,
    IMediaStorageService mediaStorageService,
    IUnitOfWork unitOfWork) : IBovineCommandService
{
    public async Task<Bovine?> Handle(CreateBovineCommand command)
    {
        if (command.StableId <= 0)
            throw new ValidationException("StableId es requerido.");

        // Verifies if the stable exists
        var stable = await stableRepository.FindByIdAsync(command.StableId);

        if (stable == null)
            throw new NotFoundException($"Stable con ID '{command.StableId}' no encontrado.");

        // Count the current bovines in the stable
        var currentBovineCount = await bovineRepository.CountBovinesByStableIdAsync(command.StableId);
        if (currentBovineCount >= stable.Limit)
        {
            throw new ValidationException("El establo está lleno. Si quiere añadir más bovinos en este establo deberá incrementar su capacidad máxima.");
        }

        // Creates a new bovine entity
        var bovineImg = mediaStorageService.UploadFileAsync(command.Name, command.FileData);
        var commandWithImg = command with { BovineImg = bovineImg };
        var bovine = new Bovine(commandWithImg);

        try
        {
            await bovineRepository.AddAsync(bovine);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return bovine;
    }

    public async Task<Bovine?> Handle(UpdateBovineCommand command)
    {
        // Verifies if the bovine exists
        var bovine = await bovineRepository.FindByIdAsync(command.Id);
        if (bovine == null)
            throw new NotFoundException($"Bovine con ID '{command.Id}' no encontrado.");

        // If stable is changing, verify capacity
        if (command.StableId.HasValue && command.StableId.Value != bovine.StableId)
        {
            var newStable = await stableRepository.FindByIdAsync(command.StableId.Value);
            if (newStable == null)
                throw new NotFoundException($"Stable con ID '{command.StableId.Value}' no encontrado.");

            var currentBovineCount = await bovineRepository.CountBovinesByStableIdAsync(command.StableId.Value);
            if (currentBovineCount >= newStable.Limit)
                throw new ValidationException("El establo está lleno. No se puede mover el bovino a este establo.");
        }

        bovine.Update(command);

        try
        {
            bovineRepository.Update(bovine);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return bovine;
    }

    public async Task<Bovine?> Handle(DeleteBovineCommand command)
    {
        // Verifies if the bovine exists
        var bovine = await bovineRepository.FindByIdAsync(command.Id);
        if (bovine == null)
            throw new NotFoundException($"Bovine con ID '{command.Id}' no encontrado.");

        try
        {
            bovineRepository.Remove(bovine);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return bovine;
    }
}