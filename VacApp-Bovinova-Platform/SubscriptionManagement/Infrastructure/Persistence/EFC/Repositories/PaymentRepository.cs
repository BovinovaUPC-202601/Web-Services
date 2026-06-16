using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Infrastructure.Persistence.EFC.Repositories;

public class PaymentRepository(AppDbContext context)
    : BaseRepository<Payment>(context), IPaymentRepository
{
    public async Task<Payment?> FindByProviderRefAsync(string providerRef)
        => await Context.Set<Payment>()
            .FirstOrDefaultAsync(p => p.ProviderRef == providerRef);

    public async Task<IEnumerable<Payment>> FindByUserIdAsync(int userId)
        => await Context.Set<Payment>()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();
}
