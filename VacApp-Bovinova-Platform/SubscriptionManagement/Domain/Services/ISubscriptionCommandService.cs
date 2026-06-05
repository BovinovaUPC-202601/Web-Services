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
}
