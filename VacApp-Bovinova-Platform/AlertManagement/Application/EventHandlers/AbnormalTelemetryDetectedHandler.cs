using MediatR;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Services;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Events;

namespace VacApp_Bovinova_Platform.AlertManagement.Application.EventHandlers;

/// <summary>
/// Reacts to AbnormalTelemetryDetectedEvent published by IoTMonitoring.
/// Creates a FEVER alert for the rancher.
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
            AlertType:    AlertType.Fever,
            UrgencyLevel: urgency,
            Message:      message
        );

        await alertCommandService.Handle(command);
    }

    private static UrgencyLevel DetermineUrgency(float temperature, float heartRate)
    {
        // RED: multiple vitals out of range or extreme deviation
        bool tempOutOfRange = temperature < 38.0f || temperature > 39.5f;
        bool hrOutOfRange   = heartRate   < 40.0f || heartRate   > 80.0f;

        if (tempOutOfRange && hrOutOfRange) return UrgencyLevel.Red;
        return UrgencyLevel.Yellow;
    }

    private static string BuildMessage(float temperature, float heartRate)
    {
        var parts = new List<string>();
        if (temperature < 38.0f) parts.Add($"temperatura baja ({temperature:F1}°C)");
        if (temperature > 39.5f) parts.Add($"fiebre ({temperature:F1}°C)");
        if (heartRate < 40.0f)   parts.Add($"bradicardia ({heartRate:F0} BPM)");
        if (heartRate > 80.0f)   parts.Add($"taquicardia ({heartRate:F0} BPM)");

        return $"Anomalía biométrica detectada: {string.Join(", ", parts)}.";
    }
}
