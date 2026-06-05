namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;

public record CollarResource(int Id, string DeviceId, int BovineId, DateTime RegisteredAt, bool IsActive);
