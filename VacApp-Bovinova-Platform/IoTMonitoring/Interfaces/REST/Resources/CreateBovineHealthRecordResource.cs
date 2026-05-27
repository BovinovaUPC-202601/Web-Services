namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;

/// <summary>
/// Payload sent by the ESP32 device to report vital signs of a bovine.
/// </summary>
public record CreateBovineHealthRecordResource(
    int    BovineId,
    string DeviceId,
    float  Temperature,
    float  HeartRate
);
