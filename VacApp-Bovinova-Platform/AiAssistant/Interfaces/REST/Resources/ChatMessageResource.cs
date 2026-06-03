namespace VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Resources;

public record ChatMessageResource(string Role, string Content, DateTime Timestamp);
