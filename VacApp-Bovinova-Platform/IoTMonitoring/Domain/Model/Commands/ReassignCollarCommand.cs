namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;

public record ReassignCollarCommand(int CollarId, int UserId, int NewBovineId);
