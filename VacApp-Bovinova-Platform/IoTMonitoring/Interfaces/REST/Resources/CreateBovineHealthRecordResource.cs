namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;

/// <summary>
/// Payload sent by the ESP32 device to report vital signs of a bovine.
/// UserId identifies the rancher who owns the bovine.
/// </summary>
public record CreateBovineHealthRecordResource(
    int    BovineId,
    int    UserId,
    string DeviceId,
    float  Temperature,
    float  HeartRate
);
