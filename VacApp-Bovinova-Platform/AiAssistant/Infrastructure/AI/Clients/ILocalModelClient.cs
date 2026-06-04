namespace VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Clients;

public interface ILocalModelClient
{
    Task<string> CompleteChatAsync(IEnumerable<object> messages);
}
