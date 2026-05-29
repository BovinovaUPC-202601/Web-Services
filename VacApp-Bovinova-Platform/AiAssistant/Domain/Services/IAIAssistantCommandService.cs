using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;

namespace VacApp_Bovinova_Platform.AIAssistant.Domain.Services;

public interface IAIAssistantCommandService
{
    Task<string> Handle(SendGeneralChatCommand command);
    Task<string> Handle(SendBovineChatCommand command);
    Task<BovineAnalysis?> Handle(AnalyzePhotoCommand command);
}
