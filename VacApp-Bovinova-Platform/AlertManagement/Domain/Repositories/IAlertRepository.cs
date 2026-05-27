using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.AlertManagement.Domain.Repositories;

public interface IAlertRepository : IBaseRepository<Alert>
{
    Task<IEnumerable<Alert>> FindByUserIdAsync(int userId);
    Task<Alert?> FindByIdAsync(int alertId);
}
