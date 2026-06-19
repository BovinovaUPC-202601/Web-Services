namespace VacApp_Bovinova_Platform.RanchManagement.Domain.Services;

public interface IProductExpiryNotificationService
{
    Task<int> SendExpiryNotificationsAsync();
}
