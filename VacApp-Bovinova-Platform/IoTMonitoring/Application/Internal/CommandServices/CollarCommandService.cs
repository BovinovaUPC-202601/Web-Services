using VacApp_Bovinova_Platform.IoTMonitoring.Application.ACL;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Repositories;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Application.Internal.CommandServices;

/// <summary>
/// Plan gating (active Plus) is enforced at the controller via [RequiresPlus];
/// this service enforces capacity, ownership and uniqueness invariants.
/// Collar allowance (3 included + approved additional requests) comes from the
/// SubscriptionManagement BC via an anti-corruption facade.
/// </summary>
public class CollarCommandService(
    ICollarRepository collarRepository,
    ISubscriptionContextFacade subscriptionContext,
    IBovineQueryService bovineQueryService,
    IUnitOfWork unitOfWork)
    : ICollarCommandService
{
    public async Task<Collar> Handle(RegisterCollarCommand command)
    {
        var allowance     = await subscriptionContext.GetCollarAllowanceAsync(command.UserId);
        var activeCollars = await collarRepository.CountActiveByUserIdAsync(command.UserId);

        if (activeCollars >= allowance)
            throw new InvalidOperationException(
                $"Collar allowance reached ({activeCollars}/{allowance}). " +
                "Request an additional collar to raise your allowance.");

        if (await collarRepository.ExistsByDeviceIdAsync(command.DeviceId))
            throw new DuplicateCollarException(command.DeviceId);

        if (await collarRepository.ExistsActiveByBovineIdAsync(command.BovineId))
            throw new InvalidOperationException($"Bovine {command.BovineId} already has a collar assigned.");

        // Ownership: the bovine must belong to the requesting user.
        var bovine = await bovineQueryService.Handle(new GetBovinesByIdQuery(command.BovineId));
        if (bovine is null || bovine.UserId != command.UserId)
            throw new InvalidOperationException($"Bovine {command.BovineId} not found or not owned by the user.");

        var collar = new Collar(command);
        await collarRepository.AddAsync(collar);
        await unitOfWork.CompleteAsync();
        return collar;
    }

    public async Task Handle(DeleteCollarCommand command)
    {
        var collar = await collarRepository.FindByIdAsync(command.CollarId);
        if (collar is null || collar.UserId != command.UserId)
            throw new InvalidOperationException($"Collar {command.CollarId} not found or not owned by the user.");

        collarRepository.Remove(collar);
        await unitOfWork.CompleteAsync();
    }

    public async Task<Collar> Handle(ReassignCollarCommand command)
    {
        var collar = await collarRepository.FindByIdAsync(command.CollarId);
        if (collar is null || collar.UserId != command.UserId)
            throw new InvalidOperationException($"Collar {command.CollarId} not found or not owned by the user.");

        // Ownership: the target bovine must belong to the requesting user.
        var bovine = await bovineQueryService.Handle(new GetBovinesByIdQuery(command.NewBovineId));
        if (bovine is null || bovine.UserId != command.UserId)
            throw new InvalidOperationException($"Bovine {command.NewBovineId} not found or not owned by the user.");

        if (collar.BovineId != command.NewBovineId
            && await collarRepository.ExistsActiveByBovineIdAsync(command.NewBovineId))
            throw new InvalidOperationException($"Bovine {command.NewBovineId} already has a collar assigned.");

        collar.Reassign(command.NewBovineId);
        collarRepository.Update(collar);
        await unitOfWork.CompleteAsync();
        return collar;
    }

    public async Task SuspendUserCollarsAsync(int userId)
    {
        var collars = await collarRepository.FindByUserIdAsync(userId);
        foreach (var collar in collars)
        {
            collar.Suspend();
            collarRepository.Update(collar);
        }
        await unitOfWork.CompleteAsync();
    }

    public async Task ReactivateUserCollarsAsync(int userId)
    {
        var collars = await collarRepository.FindByUserIdAsync(userId);
        foreach (var collar in collars)
        {
            collar.Reactivate();
            collarRepository.Update(collar);
        }
        await unitOfWork.CompleteAsync();
    }
}
