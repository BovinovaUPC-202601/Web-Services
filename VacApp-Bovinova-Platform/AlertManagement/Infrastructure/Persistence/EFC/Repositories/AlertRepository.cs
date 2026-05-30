using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VacApp_Bovinova_Platform.AlertManagement.Infrastructure.Persistence.EFC.Repositories;

public class AlertRepository(AppDbContext context)
    : BaseRepository<Alert>(context), IAlertRepository
{
    public async Task<IEnumerable<Alert>> FindByUserIdAsync(int userId)
        => await Context.Set<Alert>()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<Alert?> FindByIdAsync(int alertId)
        => await Context.Set<Alert>().FirstOrDefaultAsync(a => a.Id == alertId);
}
