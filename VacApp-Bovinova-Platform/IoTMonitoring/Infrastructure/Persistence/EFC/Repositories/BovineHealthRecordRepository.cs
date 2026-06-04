using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Infrastructure.Persistence.EFC.Repositories;

public class BovineHealthRecordRepository(AppDbContext context)
    : BaseRepository<BovineHealthRecord>(context), IBovineHealthRecordRepository
{
    public async Task<IEnumerable<BovineHealthRecord>> FindByBovineIdAsync(int bovineId, int userId)
        => await Context.Set<BovineHealthRecord>()
            .Where(r => r.BovineId == bovineId && r.UserId == userId)
            .OrderByDescending(r => r.RecordedAt)
            .ToListAsync();

    public async Task<BovineHealthRecord?> FindLatestByBovineIdAsync(int bovineId, int userId)
        => await Context.Set<BovineHealthRecord>()
            .Where(r => r.BovineId == bovineId && r.UserId == userId)
            .OrderByDescending(r => r.RecordedAt)
            .FirstOrDefaultAsync();
}
