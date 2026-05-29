using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Services;
using VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Clients;

namespace VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Services;

public class LmStudioChatService(ILocalModelClient localModelClient) : IAIChatService
{
    public async Task<string> GenerateResponseAsync(
        string systemPrompt,
        IEnumerable<ChatMessage> conversationHistory,
        string userMessage)
    {
        var messages = new List<object>
        {
            new
            {
                role = "system",
                content = systemPrompt
            }
        };

        messages.AddRange(conversationHistory.Select(message => new
        {
            role = message.Role,
            content = message.Content
        }));

        messages.Add(new
        {
            role = "user",
            content = userMessage
        });

        return await localModelClient.CompleteChatAsync(messages);
    }
}
