using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VacApp_Bovinova_Platform.AIAssistant.Infrastructure.Persistence.EFC.Repositories;

public class BovineAnalysisRepository(AppDbContext context)
    : BaseRepository<BovineAnalysis>(context), IBovineAnalysisRepository
{
    public async Task<IEnumerable<BovineAnalysis>> FindByBovineIdAsync(int bovineId)
    {
        return await Context.Set<BovineAnalysis>()
            .Where(analysis => analysis.BovineId == bovineId)
            .OrderByDescending(analysis => analysis.CreatedAt)
            .ToListAsync();
    }

    public async Task<BovineAnalysis?> FindLatestByBovineIdAsync(int bovineId)
    {
        return await Context.Set<BovineAnalysis>()
            .Where(analysis => analysis.BovineId == bovineId)
            .OrderByDescending(analysis => analysis.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
