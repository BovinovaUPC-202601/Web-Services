using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Entities;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories
{
    public interface IBovineBreedRepository : IBaseRepository<BovineBreed>
    {
    Task<IEnumerable<BovineBreed>> FindByUserIdOrGlobalAsync(int userId);
    Task<IEnumerable<BovineBreed>> FindGlobalAsync();
}
}