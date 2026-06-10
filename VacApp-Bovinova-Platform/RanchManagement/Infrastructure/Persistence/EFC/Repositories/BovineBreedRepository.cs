using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Entities;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VacApp_Bovinova_Platform.RanchManagement.Infrastructure.Persistence.EFC.Repositories
{
    public class BovineBreedRepository(AppDbContext ctx) : BaseRepository<BovineBreed>(ctx), IBovineBreedRepository
    {

    }
}