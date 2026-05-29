using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.AlertManagement.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.AlertManagement.Interfaces.REST.Transform;

public static class AlertResourceFromEntityAssembler
{
    public static AlertResource ToResourceFromEntity(Alert alert)
        => new(
            alert.Id,
            alert.BovineId,
            alert.UserId,
            alert.AlertType.ToString(),
            alert.UrgencyLevel.ToString(),
            alert.Status.ToString(),
            alert.Message,
            alert.CreatedAt
        );
}
