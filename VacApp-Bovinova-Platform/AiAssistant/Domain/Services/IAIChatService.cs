using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.AIAssistant.Domain.Services;

public interface IAIChatService
{
    Task<string> GenerateResponseAsync(
        string systemPrompt,
        IEnumerable<ChatMessage> conversationHistory,
        string userMessage);
}
