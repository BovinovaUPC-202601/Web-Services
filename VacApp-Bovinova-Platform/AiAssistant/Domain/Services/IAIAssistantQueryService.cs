using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Queries;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.AIAssistant.Domain.Services;

public interface IAIAssistantQueryService
{
    Task<IReadOnlyCollection<ChatMessage>> Handle(GetGeneralChatHistoryQuery query);
    Task<IReadOnlyCollection<ChatMessage>> Handle(GetBovineChatHistoryQuery query);
    Task<IEnumerable<BovineAnalysis>> Handle(GetBovineAnalysesQuery query);
}
