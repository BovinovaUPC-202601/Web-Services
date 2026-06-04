namespace VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Resources;

public record ChatHistoryResource(
    string ConversationType,
    int? BovineId,
    IEnumerable<ChatMessageResource> Messages);
