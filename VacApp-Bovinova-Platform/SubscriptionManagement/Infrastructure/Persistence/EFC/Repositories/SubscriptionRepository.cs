using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Infrastructure.Persistence.EFC.Repositories;

public class SubscriptionRepository(AppDbContext context)
    : BaseRepository<Subscription>(context), ISubscriptionRepository
{
    public async Task<Subscription?> FindByUserIdAsync(int userId)
        => await Context.Set<Subscription>()
            .FirstOrDefaultAsync(s => s.UserId == userId);

    public async Task<IEnumerable<Subscription>> FindInactiveAsync()
        => await Context.Set<Subscription>()
            .Where(s => s.Status == SubscriptionStatus.Suspended || s.Status == SubscriptionStatus.Cancelled)
            .ToListAsync();

    public async Task<IEnumerable<Subscription>> FindActivePlusWithRenewalAsync()
        => await Context.Set<Subscription>()
            .Where(s => s.Plan == PlanType.Plus
                        && s.Status == SubscriptionStatus.Active
                        && s.NextRenewal != null)
            .ToListAsync();
}
