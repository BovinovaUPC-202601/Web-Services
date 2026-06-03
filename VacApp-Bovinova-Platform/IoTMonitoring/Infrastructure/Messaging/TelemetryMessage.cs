namespace VacApp_Bovinova_Platform.IoTMonitoring.Infrastructure.Messaging;

/// <summary>
/// Shape of the JSON the collar (ESP32) publishes to the telemetry topic.
/// Mirrors User Story E01: deviceId, temperature, heartRate, batteryLevel, timestamp.
/// </summary>
public class TelemetryMessage
{
    public int    BovineId     { get; set; }
    public int    UserId       { get; set; }
    public string DeviceId     { get; set; } = string.Empty;
    public float  Temperature  { get; set; }
    public float  HeartRate    { get; set; }
    public int    BatteryLevel { get; set; }
    public string? Timestamp   { get; set; } // informational; server stamps RecordedAt
}

/// <summary>
/// Shape of the JSON the backend publishes back to the device-specific
/// response topic. The collar uses <see cref="IsAlert"/> to drive its LED.
/// </summary>
public class TelemetryResponse
{
    public int    Id      { get; set; }
    public bool   IsAlert { get; set; }
    public string Message { get; set; } = string.Empty;
}
