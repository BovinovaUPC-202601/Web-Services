namespace VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Commands;

// UserId is the real signed-in user. EffectiveUserId is the ranch owner (the staff member's
// boss when applicable); the resulting analysis is stored under the ranch so the whole team sees it.
public record AnalyzePhotoCommand(int UserId, int EffectiveUserId, int BovineId, string ImageBase64);
