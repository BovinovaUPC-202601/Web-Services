namespace VacApp_Bovinova_Platform.AlertManagement.Interfaces.REST.Resources;

public record AlertResource(
    int    Id,
    int?   BovineId,
    int    UserId,
    string AlertType,
    string UrgencyLevel,
    string Status,
    string Message,
    DateTime CreatedAt
);
