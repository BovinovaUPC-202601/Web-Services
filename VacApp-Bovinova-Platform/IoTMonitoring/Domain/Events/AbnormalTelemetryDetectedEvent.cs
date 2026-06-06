using MediatR;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Events;

/// <summary>
/// Published by IoTMonitoring when a bovine's vital signs are outside normal range.
/// AlertManagement subscribes to this event to create a FEVER alert.
/// </summary>
public record AbnormalTelemetryDetectedEvent(
    int    BovineId,
    int    UserId,
    float  Temperature,
    float  HeartRate,
    string DeviceId,
    float  MinTemperature,
    float  MaxTemperature,
    float  MinHeartRate,
    float  MaxHeartRate
) : INotification;
