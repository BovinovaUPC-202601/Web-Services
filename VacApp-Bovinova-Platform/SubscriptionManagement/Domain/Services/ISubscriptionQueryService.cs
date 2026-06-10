using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Queries;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;

public interface ISubscriptionQueryService
{
    Task<Subscription?> Handle(GetSubscriptionByUserIdQuery query);

    /// <summary>Collar allowance = included collars (3) + approved additional requests.</summary>
    Task<int> GetCollarAllowanceAsync(int userId);
}
