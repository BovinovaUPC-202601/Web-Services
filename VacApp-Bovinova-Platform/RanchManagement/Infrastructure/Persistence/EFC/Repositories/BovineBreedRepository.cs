using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Entities;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VacApp_Bovinova_Platform.RanchManagement.Infrastructure.Persistence.EFC.Repositories
{
    public class BovineBreedRepository(AppDbContext ctx) : BaseRepository<BovineBreed>(ctx), IBovineBreedRepository
    {
        public async Task<IEnumerable<BovineBreed>> FindByUserIdOrGlobalAsync(int userId)
            => await ctx.Set<BovineBreed>()
                .Where(b => b.UserId == null || b.UserId == userId)
                .ToListAsync();

        public async Task<IEnumerable<BovineBreed>> FindGlobalAsync()
            => await ctx.Set<BovineBreed>()
                .Where(b => b.UserId == null)
                .ToListAsync();
    }
}