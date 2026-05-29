using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VacApp_Bovinova_Platform.RanchManagement.Infrastructure.Persistence.EFC.Repositories;

public class ProductRepository(AppDbContext ctx) : BaseRepository<Product>(ctx), IProductRepository
{
    public async Task<IEnumerable<Product>> FindByUserIdAsync(int userId)
        => await ctx.Products.Where(p => p.UserId == userId).ToListAsync();

    public async Task<IEnumerable<Product>> FindByCategoryIdAsync(int categoryId)
        => await ctx.Products.Where(p => p.CategoryId == categoryId).ToListAsync();

    public async Task<IEnumerable<Product>> GetAllAsync()
        => await ctx.Products.ToListAsync();
}
