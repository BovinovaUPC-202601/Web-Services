using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Entities;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;

namespace VacApp_Bovinova_Platform.RanchManagement.Application.Internal.QueryServices;

public class BovineQueryService(IBovineRepository bovineRepository, IBovineBreedRepository bovineBreedRepository) : IBovineQueryService
{
    public async Task<IEnumerable<Bovine>> Handle(GetAllBovinesQuery query)
    {
        return await bovineRepository.FindByUserIdAsync(query.UserId);
    }

    public async Task<Bovine> Handle(GetBovinesByIdQuery query)
    {
        return await bovineRepository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<Bovine>> Handle(GetBovinesByStableIdQuery query)
    {
        return await bovineRepository.FindByStableIdAsync(query.StableId);
    }

    public async Task<int> CountBovinesByUserIdAsync(int userId)
    {
        var bovines = await bovineRepository.FindByUserIdAsync(userId);
        return bovines.Count();
    }

    public async Task<IEnumerable<BovineBreed>> Handle(GetAllBovineBreedsQuery query)
    {
        return await bovineBreedRepository.ListAsync();
    }
}
