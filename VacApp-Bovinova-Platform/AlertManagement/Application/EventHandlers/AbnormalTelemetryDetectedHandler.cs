using MediatR;
using Microsoft.Extensions.Logging;
using VacApp_Bovinova_Platform.AlertManagement.Application.Outbound;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Services;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Events;
using VacApp_Bovinova_Platform.Shared.Domain.Model;

namespace VacApp_Bovinova_Platform.AlertManagement.Application.EventHandlers;

/// <summary>
/// Reacts to AbnormalTelemetryDetectedEvent published by IoTMonitoring.
/// Creates a biometric-anomaly alert for the rancher; the specific condition
/// (fever, hypothermia, tachycardia, …) is described in the alert message.
/// AlertManagement does NOT depend on IoTMonitoring — only on the shared event contract.
/// </summary>
public class AbnormalTelemetryDetectedHandler(
    IAlertCommandService alertCommandService,
    IPushNotificationService pushNotificationService,
    ILogger<AbnormalTelemetryDetectedHandler> logger)
    : INotificationHandler<AbnormalTelemetryDetectedEvent>
{
    public async Task Handle(AbnormalTelemetryDetectedEvent notification, CancellationToken cancellationToken)
    {
        var message = BuildMessage(notification);
        var urgency = DetermineUrgency(notification);

        var command = new RegisterAlertCommand(
            BovineId:     notification.BovineId,
            UserId:       notification.UserId,
            AlertType:    AlertType.BiometricAnomaly,
            UrgencyLevel: urgency,
            Message:      message
        );

        var alert = await alertCommandService.Handle(command);
        if (alert is null) return;

        // Best-effort background push. A delivery failure must never break alert creation.
        var title = urgency == UrgencyLevel.Red ? "🔴 Alerta biométrica crítica" : "🟡 Alerta biométrica";
        try
        {
            await pushNotificationService.SendToUserAsync(notification.UserId, title, message, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Push notification failed for user {UserId}", notification.UserId);
        }
    }

    private static UrgencyLevel DetermineUrgency(AbnormalTelemetryDetectedEvent n)
    {
        // RED: both vitals out of range; YELLOW: a single vital out of range.
        // Judged against the bovine's own thresholds carried by the event.
        bool tempOutOfRange = BovineVitalRanges.IsTemperatureOutOfRange(n.Temperature, n.MinTemperature, n.MaxTemperature);
        bool hrOutOfRange   = BovineVitalRanges.IsHeartRateOutOfRange(n.HeartRate, n.MinHeartRate, n.MaxHeartRate);

        if (tempOutOfRange && hrOutOfRange) return UrgencyLevel.Red;
        return UrgencyLevel.Yellow;
    }

    private static string BuildMessage(AbnormalTelemetryDetectedEvent n)
    {
        var parts = new List<string>();
        if (n.Temperature < n.MinTemperature) parts.Add($"temperatura baja ({n.Temperature:F1}°C)");
        if (n.Temperature > n.MaxTemperature) parts.Add($"fiebre ({n.Temperature:F1}°C)");
        if (n.HeartRate < n.MinHeartRate)     parts.Add($"bradicardia ({n.HeartRate:F0} BPM)");
        if (n.HeartRate > n.MaxHeartRate)     parts.Add($"taquicardia ({n.HeartRate:F0} BPM)");

        return $"Anomalía biométrica detectada: {string.Join(", ", parts)}.";
    }
}
