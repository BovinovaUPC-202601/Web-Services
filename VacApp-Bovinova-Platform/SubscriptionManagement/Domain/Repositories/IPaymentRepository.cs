using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Repositories;

public interface IPaymentRepository : IBaseRepository<Payment>
{
    /// <summary>Finds a payment by its gateway reference (checkout session id).</summary>
    Task<Payment?> FindByProviderRefAsync(string providerRef);

    /// <summary>Billing history for a user, newest first.</summary>
    Task<IEnumerable<Payment>> FindByUserIdAsync(int userId);
}
