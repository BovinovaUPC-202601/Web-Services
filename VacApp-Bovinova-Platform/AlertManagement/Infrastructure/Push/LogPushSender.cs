using Microsoft.Extensions.Logging;
using VacApp_Bovinova_Platform.AlertManagement.Application.Outbound;

namespace VacApp_Bovinova_Platform.AlertManagement.Infrastructure.Push;

/// <summary>
/// No-op <see cref="IPushNotificationService"/>: logs the push instead of sending it. Used
/// as the default when ONESIGNAL_REST_API_KEY is absent, so the demo keeps working with
/// zero external config (the push still shows up in the backend log).
/// </summary>
public class LogPushSender(ILogger<LogPushSender> logger) : IPushNotificationService
{
    public Task SendToUserAsync(int userId, string title, string message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[PUSH:noop] would send to user {UserId} | title: {Title} | message: {Message}",
            userId, title, message);
        return Task.CompletedTask;
    }
}
