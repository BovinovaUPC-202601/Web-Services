namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;

public record CreateBovineHealthRecordCommand(
    int    BovineId,
    int    UserId,
    string DeviceId,
    float  Temperature,
    float  HeartRate
);
