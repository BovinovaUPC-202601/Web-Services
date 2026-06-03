namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;

/// <summary>
/// Read model returned to the mobile app / dashboard.
/// </summary>
public record BovineHealthRecordResource(
    int      Id,
    int      BovineId,
    string   DeviceId,
    float    Temperature,
    float    HeartRate,
    int      BatteryLevel,
    bool     IsAlert,
    DateTime RecordedAt
);
