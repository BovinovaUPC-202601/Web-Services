using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Infrastructure.Persistence.EFC.Repositories;

public class AdditionalCollarRequestRepository(AppDbContext context)
    : BaseRepository<AdditionalCollarRequest>(context), IAdditionalCollarRequestRepository
{
    public async Task<IEnumerable<AdditionalCollarRequest>> FindByUserIdAsync(int userId)
        => await Context.Set<AdditionalCollarRequest>()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

    public async Task<int> CountActiveByUserIdAsync(int userId)
        => await Context.Set<AdditionalCollarRequest>()
            .CountAsync(r => r.UserId == userId &&
                (r.Status == AdditionalCollarStatus.Approved || r.Status == AdditionalCollarStatus.Delivered));
}
