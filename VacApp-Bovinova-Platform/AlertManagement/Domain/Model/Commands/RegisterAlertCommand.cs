using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Commands;

public record RegisterAlertCommand(
    int          BovineId,
    int          UserId,
    AlertType    AlertType,
    UrgencyLevel UrgencyLevel,
    string       Message
);
