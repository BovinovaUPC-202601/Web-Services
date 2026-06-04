namespace VacApp_Bovinova_Platform.AIAssistant.Application.ACL;

public interface IAlertContextFacade
{
    Task<string> GetBovineAlertContextAsync(int userId, int bovineId);
}
