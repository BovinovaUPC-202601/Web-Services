using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.AIAssistant.Domain.Repositories;

public interface IBovineAnalysisRepository : IBaseRepository<BovineAnalysis>
{
    Task<IEnumerable<BovineAnalysis>> FindByBovineIdAsync(int bovineId);
    Task<IEnumerable<BovineAnalysis>> FindByUserIdAndBovineIdAsync(int userId, int bovineId);
    Task<BovineAnalysis?> FindLatestByBovineIdAsync(int bovineId);
}
