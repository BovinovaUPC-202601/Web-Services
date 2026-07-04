using MediatR;
using Microsoft.Extensions.Logging;
using VacApp_Bovinova_Platform.AlertManagement.Application.Outbound;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Services;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Events;

namespace VacApp_Bovinova_Platform.AlertManagement.Application.EventHandlers;

/// <summary>
/// Reacts to SubscriptionEndedEvent published by SubscriptionManagement when a Plus plan
/// is suspended. Raises an account-level collar-return alert so the user knows they must
/// hand the IoT collars back. AlertManagement depends only on the shared event contract.
/// </summary>
public class SubscriptionEndedHandler(
    IAlertCommandService alertCommandService,
    IPushNotificationService pushNotificationService,
    ILogger<SubscriptionEndedHandler> logger)
    : INotificationHandler<SubscriptionEndedEvent>
{
    public async Task Handle(SubscriptionEndedEvent notification, CancellationToken cancellationToken)
    {
        var command = new RegisterCollarReturnAlertCommand(
            UserId:      notification.UserId,
            CollarCount: notification.CollarCount);

        var alert = await alertCommandService.Handle(command);
        if (alert is null) return;

        // Best-effort background push. A delivery failure must never break alert creation.
        var message = $"Tu plan Plus fue suspendido. Debes devolver {notification.CollarCount} collar(es) IoT.";
        try
        {
            await pushNotificationService.SendToUserAsync(
                notification.UserId, "Devolución de collares", message, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Push notification failed for user {UserId}", notification.UserId);
        }
    }
}
