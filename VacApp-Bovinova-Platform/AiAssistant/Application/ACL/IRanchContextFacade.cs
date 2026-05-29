namespace VacApp_Bovinova_Platform.AIAssistant.Application.ACL;

public interface IRanchContextFacade
{
    Task<string> GetGeneralRanchContextAsync(int userId);
    Task<string> GetBovineContextAsync(int userId, int bovineId);
}
