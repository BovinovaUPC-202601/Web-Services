using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Infrastructure.Persistence.EFC.Repositories;

public class CollarRepository(AppDbContext context)
    : BaseRepository<Collar>(context), ICollarRepository
{
    public async Task<IEnumerable<Collar>> FindByUserIdAsync(int userId)
        => await Context.Set<Collar>()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.RegisteredAt)
            .ToListAsync();

    public async Task<int> CountActiveByUserIdAsync(int userId)
        => await Context.Set<Collar>()
            .CountAsync(c => c.UserId == userId && c.LifecycleStatus == CollarLifecycleStatus.Active);

    public async Task<bool> ExistsByDeviceIdAsync(string deviceId)
        => await Context.Set<Collar>()
            .AnyAsync(c => c.DeviceId == deviceId);

    public async Task<bool> ExistsActiveByBovineIdAsync(int bovineId)
        => await Context.Set<Collar>()
            .AnyAsync(c => c.BovineId == bovineId && c.LifecycleStatus == CollarLifecycleStatus.Active);
}
