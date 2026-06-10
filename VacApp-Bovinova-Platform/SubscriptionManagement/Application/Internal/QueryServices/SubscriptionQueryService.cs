using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Application.Internal.QueryServices;

public class SubscriptionQueryService(
    ISubscriptionRepository subscriptionRepository,
    IAdditionalCollarRequestRepository additionalCollarRepository)
    : ISubscriptionQueryService
{
    public async Task<Subscription?> Handle(GetSubscriptionByUserIdQuery query)
        => await subscriptionRepository.FindByUserIdAsync(query.UserId);

    public async Task<int> GetCollarAllowanceAsync(int userId)
    {
        var subscription = await subscriptionRepository.FindByUserIdAsync(userId);
        if (subscription is null || !subscription.IsPlusActive)
            return 0; // Free / inactive: no collars

        var extra = await additionalCollarRepository.CountActiveByUserIdAsync(userId);
        return SubscriptionPricing.IncludedCollars + extra;
    }
}
