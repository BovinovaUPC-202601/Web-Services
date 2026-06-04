using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Repositories;

public interface IBovineHealthRecordRepository : IBaseRepository<BovineHealthRecord>
{
    Task<IEnumerable<BovineHealthRecord>> FindByBovineIdAsync(int bovineId, int userId);
    Task<BovineHealthRecord?> FindLatestByBovineIdAsync(int bovineId, int userId);
}
