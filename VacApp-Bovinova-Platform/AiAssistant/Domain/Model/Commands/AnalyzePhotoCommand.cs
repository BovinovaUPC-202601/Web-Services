namespace VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Commands;

public record AnalyzePhotoCommand(int UserId, int BovineId, string ImageBase64);
