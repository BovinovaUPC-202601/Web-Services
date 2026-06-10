using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Repositories;

public interface IAdditionalCollarRequestRepository : IBaseRepository<AdditionalCollarRequest>
{
    Task<IEnumerable<AdditionalCollarRequest>> FindByUserIdAsync(int userId);

    /// <summary>Count of approved/delivered requests — the extra collar slots granted.</summary>
    Task<int> CountActiveByUserIdAsync(int userId);
}
