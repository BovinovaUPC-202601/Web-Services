using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<IEnumerable<Product>> FindByUserIdAsync(int userId);
    Task<IEnumerable<Product>> FindByCategoryIdAsync(int categoryId);
    Task<IEnumerable<Product>> FindByExpirationDateWindowAsync(DateOnly from, DateOnly to);
}
