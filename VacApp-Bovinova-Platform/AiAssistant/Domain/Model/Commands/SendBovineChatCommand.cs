namespace VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Commands;

public record SendBovineChatCommand(int UserId, int BovineId, string Message);
