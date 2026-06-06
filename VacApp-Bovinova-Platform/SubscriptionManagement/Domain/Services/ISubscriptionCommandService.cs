using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Commands;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;

public interface ISubscriptionCommandService
{
    Task<Subscription> Handle(ActivatePlusCommand command);
    Task<AdditionalCollarRequest> Handle(RequestAdditionalCollarCommand command);
    Task<Subscription?> Handle(SuspendSubscriptionCommand command);
    Task<Subscription?> Handle(CancelSubscriptionCommand command);
    Task<AdditionalCollarRequest?> Handle(ApproveAdditionalCollarCommand command);
    Task<AdditionalCollarRequest?> Handle(DeliverAdditionalCollarCommand command);

    /// <summary>
    /// Reconciles the denormalized IAM SubscriptionPlan flag with the authoritative
    /// subscription aggregate. Heals any drift (e.g. an activation that failed to
    /// sync the flag) so [RequiresPlus] reflects the real plan. Idempotent.
    /// </summary>
    Task SyncIamPlanAsync(int userId);
}
