using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Commands;

namespace VacApp_Bovinova_Platform.AlertManagement.Domain.Services;

public interface IAlertCommandService
{
    Task<Alert?> Handle(RegisterAlertCommand command);
    Task<Alert?> Handle(MarkAlertAsReadCommand command);
}
