using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Repositories;

public interface ICollarRepository : IBaseRepository<Collar>
{
    Task<IEnumerable<Collar>> FindByUserIdAsync(int userId);
    Task<int> CountActiveByUserIdAsync(int userId);
    Task<bool> ExistsByDeviceIdAsync(string deviceId);
    Task<bool> ExistsActiveByBovineIdAsync(int bovineId);
}
