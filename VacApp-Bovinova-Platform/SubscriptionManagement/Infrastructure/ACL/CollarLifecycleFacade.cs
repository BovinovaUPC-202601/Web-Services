using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;
using VacApp_Bovinova_Platform.SubscriptionManagement.Application.ACL;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Infrastructure.ACL;

public class CollarLifecycleFacade(ICollarCommandService collarCommandService)
    : ICollarLifecycleFacade
{
    public Task SuspendUserCollarsAsync(int userId)
        => collarCommandService.SuspendUserCollarsAsync(userId);

    public Task ReactivateUserCollarsAsync(int userId)
        => collarCommandService.ReactivateUserCollarsAsync(userId);
}
