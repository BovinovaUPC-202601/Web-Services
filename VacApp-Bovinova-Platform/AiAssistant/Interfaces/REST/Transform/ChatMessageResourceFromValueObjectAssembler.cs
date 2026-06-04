using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Transform;

public static class ChatMessageResourceFromValueObjectAssembler
{
    public static ChatMessageResource ToResourceFromValueObject(ChatMessage message)
    {
        return new ChatMessageResource(message.Role, message.Content, message.Timestamp);
    }
}
