namespace VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Commands;

// UserId owns the chat session/history (the real signed-in user, e.g. a staff member).
// EffectiveUserId scopes the ranch data fed to the model (the ranch owner when a staff member is signed in).
public record SendBovineChatCommand(int UserId, int EffectiveUserId, int BovineId, string Message);
