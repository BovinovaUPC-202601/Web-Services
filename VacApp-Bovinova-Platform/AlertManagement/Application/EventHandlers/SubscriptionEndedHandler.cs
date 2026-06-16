using MediatR;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Services;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Events;

namespace VacApp_Bovinova_Platform.AlertManagement.Application.EventHandlers;

/// <summary>
/// Reacts to SubscriptionEndedEvent published by SubscriptionManagement when a Plus plan
/// is suspended. Raises an account-level collar-return alert so the user knows they must
/// hand the IoT collars back. AlertManagement depends only on the shared event contract.
/// </summary>
public class SubscriptionEndedHandler(IAlertCommandService alertCommandService)
    : INotificationHandler<SubscriptionEndedEvent>
{
    public async Task Handle(SubscriptionEndedEvent notification, CancellationToken cancellationToken)
    {
        var command = new RegisterCollarReturnAlertCommand(
            UserId:      notification.UserId,
            CollarCount: notification.CollarCount);

        await alertCommandService.Handle(command);
    }
}
