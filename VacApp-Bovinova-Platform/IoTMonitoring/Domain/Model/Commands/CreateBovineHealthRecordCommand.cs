namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;

/// <summary>
/// Command sent by the ESP32 device with the bovine's vital signs reading.
/// </summary>
public record CreateBovineHealthRecordCommand(
    int    BovineId,
    string DeviceId,
    float  Temperature,
    float  HeartRate
);
