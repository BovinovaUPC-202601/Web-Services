namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;

public record RegisterCollarCommand(int UserId, string DeviceId, int BovineId);
