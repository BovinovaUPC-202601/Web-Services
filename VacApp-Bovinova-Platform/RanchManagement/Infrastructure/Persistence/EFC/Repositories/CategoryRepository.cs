using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VacApp_Bovinova_Platform.RanchManagement.Infrastructure.Persistence.EFC.Repositories;

public class CategoryRepository(AppDbContext ctx) : BaseRepository<Category>(ctx), ICategoryRepository
{
    public async Task<IEnumerable<Category>> FindByUserIdAsync(int userId)
        => await ctx.Categories.Where(c => c.UserId == userId).ToListAsync();

    public async Task<IEnumerable<Category>> GetAllAsync()
        => await ctx.Categories.ToListAsync();
}
