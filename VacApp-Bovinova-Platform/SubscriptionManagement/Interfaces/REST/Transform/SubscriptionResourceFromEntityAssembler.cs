using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.SubscriptionManagement.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Interfaces.REST.Transform;

public static class SubscriptionResourceFromEntityAssembler
{
    public static SubscriptionResource ToResourceFromEntity(Subscription subscription, int activeCollars)
    {
        var additional = Math.Max(0, activeCollars - SubscriptionPricing.IncludedCollars);
        var monthlyCost = subscription.IsPlusActive
            ? SubscriptionPricing.MonthlyCost(activeCollars)
            : 0m;

        return new SubscriptionResource(
            subscription.Plan.ToString(),
            subscription.Status.ToString(),
            subscription.StartDate,
            subscription.NextRenewal,
            subscription.Plan == PlanType.Plus ? SubscriptionPricing.IncludedCollars : 0,
            additional,
            monthlyCost);
    }
}
