using MediatR;
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
public class AbnormalTelemetryDetectedHandler(IAlertCommandService alertCommandService)
    : INotificationHandler<AbnormalTelemetryDetectedEvent>
{
    public async Task Handle(AbnormalTelemetryDetectedEvent notification, CancellationToken cancellationToken)
    {
        var message = BuildMessage(notification.Temperature, notification.HeartRate);
        var urgency = DetermineUrgency(notification.Temperature, notification.HeartRate);

        var command = new RegisterAlertCommand(
            BovineId:     notification.BovineId,
            UserId:       notification.UserId,
            AlertType:    AlertType.BiometricAnomaly,
            UrgencyLevel: urgency,
            Message:      message
        );

        await alertCommandService.Handle(command);
    }

    private static UrgencyLevel DetermineUrgency(float temperature, float heartRate)
    {
        // RED: both vitals out of range; YELLOW: a single vital out of range
        bool tempOutOfRange = BovineVitalRanges.IsTemperatureOutOfRange(temperature);
        bool hrOutOfRange   = BovineVitalRanges.IsHeartRateOutOfRange(heartRate);

        if (tempOutOfRange && hrOutOfRange) return UrgencyLevel.Red;
        return UrgencyLevel.Yellow;
    }

    private static string BuildMessage(float temperature, float heartRate)
    {
        var parts = new List<string>();
        if (temperature < BovineVitalRanges.MinTemperature) parts.Add($"temperatura baja ({temperature:F1}°C)");
        if (temperature > BovineVitalRanges.MaxTemperature) parts.Add($"fiebre ({temperature:F1}°C)");
        if (heartRate < BovineVitalRanges.MinHeartRate)     parts.Add($"bradicardia ({heartRate:F0} BPM)");
        if (heartRate > BovineVitalRanges.MaxHeartRate)     parts.Add($"taquicardia ({heartRate:F0} BPM)");

        return $"Anomalía biométrica detectada: {string.Join(", ", parts)}.";
    }
}
