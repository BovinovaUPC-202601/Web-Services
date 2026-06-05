using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Queries;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;

public interface ICollarQueryService
{
    Task<IEnumerable<Collar>> Handle(GetCollarsByUserIdQuery query);

    /// <summary>Active collars currently registered by the user (counts toward billing).</summary>
    Task<int> GetActiveCountAsync(int userId);
}
