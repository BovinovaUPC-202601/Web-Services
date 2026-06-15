using MediatR;
using VacApp_Bovinova_Platform.IAM.Domain.Model;
using VacApp_Bovinova_Platform.IAM.Domain.Repositories;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Queries;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Application.ACL;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Events;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Application.Internal.CommandServices;

public class SubscriptionCommandService(
    ISubscriptionRepository subscriptionRepository,
    IAdditionalCollarRequestRepository additionalCollarRepository,
    IUserRepository userRepository,
    ICollarLifecycleFacade collarLifecycle,
    ICollarQueryService collarQueryService,
    IMediator mediator,
    IUnitOfWork unitOfWork)
    : ISubscriptionCommandService
{
    public async Task<Subscription> Handle(ActivatePlusCommand command)
    {
        var subscription = await subscriptionRepository.FindByUserIdAsync(command.UserId);
        if (subscription is null)
        {
            subscription = new Subscription(command.UserId);
            await subscriptionRepository.AddAsync(subscription);
        }

        subscription.ActivatePlus();

        // Keep the legacy IAM flag in sync so [RequiresPlus] reflects effective access.
        var user = await userRepository.FindByIdAsync(command.UserId);
        user?.ChangeSubscription(SubscriptionPlans.Plus);

        await unitOfWork.CompleteAsync();
        // Reactivate any collars that were suspended while the subscription was inactive.
        await collarLifecycle.ReactivateUserCollarsAsync(command.UserId);
        return subscription;
    }

    public async Task SyncIamPlanAsync(int userId)
    {
        var subscription = await subscriptionRepository.FindByUserIdAsync(userId);
        var effectivePlan = subscription is { IsPlusActive: true }
            ? SubscriptionPlans.Plus
            : SubscriptionPlans.Free;

        var user = await userRepository.FindByIdAsync(userId);
        if (user is null || user.SubscriptionPlan == effectivePlan) return;

        // Drift detected: the denormalized IAM flag disagrees with the aggregate. Heal it.
        user.ChangeSubscription(effectivePlan);
        await unitOfWork.CompleteAsync();
    }

    public async Task<AdditionalCollarRequest> Handle(RequestAdditionalCollarCommand command)
    {
        var subscription = await subscriptionRepository.FindByUserIdAsync(command.UserId);
        if (subscription is null || !subscription.IsPlusActive)
            throw new InvalidOperationException("An active Plus subscription is required to request additional collars.");

        // Created as pending; an admin approves and delivers it (TP TS023).
        var request = new AdditionalCollarRequest(command.UserId, SubscriptionPricing.AdditionalCollarMonthly);
        await additionalCollarRepository.AddAsync(request);
        await unitOfWork.CompleteAsync();
        return request;
    }

    public async Task<Subscription?> Handle(SuspendSubscriptionCommand command)
    {
        var subscription = await subscriptionRepository.FindByUserIdAsync(command.UserId);
        if (subscription is null) return null;

        subscription.Suspend();

        // Conserve only Free access: drop the gate flag and suspend the collars.
        var user = await userRepository.FindByIdAsync(command.UserId);
        user?.ChangeSubscription(SubscriptionPlans.Free);

        await unitOfWork.CompleteAsync();
        await collarLifecycle.SuspendUserCollarsAsync(command.UserId);

        // Plan ended → tell the user to return their collars (account-level alert).
        var collars = await collarQueryService.Handle(new GetCollarsByUserIdQuery(command.UserId));
        await mediator.Publish(new SubscriptionEndedEvent(command.UserId, collars.Count()));
        return subscription;
    }

    public async Task<Subscription?> Handle(CancelSubscriptionCommand command)
    {
        var subscription = await subscriptionRepository.FindByUserIdAsync(command.UserId);
        if (subscription is null) return null;

        subscription.Cancel();

        var user = await userRepository.FindByIdAsync(command.UserId);
        user?.ChangeSubscription(SubscriptionPlans.Free);

        await unitOfWork.CompleteAsync();
        await collarLifecycle.SuspendUserCollarsAsync(command.UserId);
        return subscription;
    }

    public async Task<AdditionalCollarRequest?> Handle(ApproveAdditionalCollarCommand command)
    {
        var request = await additionalCollarRepository.FindByIdAsync(command.RequestId);
        if (request is null) return null;

        request.Approve();
        await unitOfWork.CompleteAsync();
        return request;
    }

    public async Task<AdditionalCollarRequest?> Handle(DeliverAdditionalCollarCommand command)
    {
        var request = await additionalCollarRepository.FindByIdAsync(command.RequestId);
        if (request is null) return null;

        request.Deliver();
        await unitOfWork.CompleteAsync();
        return request;
    }
}
