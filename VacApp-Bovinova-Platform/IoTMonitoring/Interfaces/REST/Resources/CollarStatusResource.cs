namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;

public record CollarStatusResource(
    int Id,
    string DeviceId,
    int BovineId,
    string OperationalStatus,
    string LifecycleStatus,
    float? LastTemperature,
    float? LastHeartRate,
    int? BatteryLevel,
    DateTime? LastSeenAt,
    DateTime RegisteredAt);
