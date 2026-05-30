namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;

/// <summary>
/// Response the ESP32 reads after posting telemetry.
/// The device uses <see cref="Alarm"/> to activate the LED actuator.
/// </summary>
public record TelemetryResponseResource(
    int    Id,
    bool   Alarm,
    string Message
);
