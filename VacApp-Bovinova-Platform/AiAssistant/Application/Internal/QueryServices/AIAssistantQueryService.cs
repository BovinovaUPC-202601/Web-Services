using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Queries;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Repositories;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Services;

namespace VacApp_Bovinova_Platform.AIAssistant.Application.Internal.QueryServices;

public class AIAssistantQueryService(
    IAISessionRepository aiSessionRepository,
    IBovineAnalysisRepository bovineAnalysisRepository) : IAIAssistantQueryService
{
    private static readonly IReadOnlyCollection<ChatMessage> EmptyHistory = Array.Empty<ChatMessage>();

    public async Task<IReadOnlyCollection<ChatMessage>> Handle(GetGeneralChatHistoryQuery query)
    {
        var session = await aiSessionRepository.FindGeneralChatSessionByUserIdAsync(query.UserId);
        return session?.GetMessages() ?? EmptyHistory;
    }

    public async Task<IReadOnlyCollection<ChatMessage>> Handle(GetBovineChatHistoryQuery query)
    {
        var session = await aiSessionRepository.FindBovineChatSessionByUserIdAndBovineIdAsync(query.UserId, query.BovineId);
        return session?.GetMessages() ?? EmptyHistory;
    }

    public async Task<IEnumerable<BovineAnalysis>> Handle(GetBovineAnalysesQuery query)
    {
        return await bovineAnalysisRepository.FindByUserIdAndBovineIdAsync(query.UserId, query.BovineId);
    }
}
