using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Queries;

namespace VacApp_Bovinova_Platform.AlertManagement.Domain.Services;

public interface IAlertQueryService
{
    Task<IEnumerable<Alert>> Handle(GetAlertsByUserIdQuery query);
    Task<Alert?>             Handle(GetAlertByIdQuery query);
}
