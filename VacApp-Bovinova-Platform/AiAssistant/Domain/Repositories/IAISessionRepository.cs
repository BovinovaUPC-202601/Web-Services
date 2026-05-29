using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.AIAssistant.Domain.Repositories;

public interface IAISessionRepository : IBaseRepository<AISession>
{
    Task<GeneralChatSession?> FindGeneralChatSessionByUserIdAsync(int userId);
    Task<BovineChatSession?> FindBovineChatSessionByUserIdAndBovineIdAsync(int userId, int bovineId);
    Task AddGeneralChatSessionAsync(GeneralChatSession session);
    Task AddBovineChatSessionAsync(BovineChatSession session);
    void UpdateGeneralChatSession(GeneralChatSession session);
    void UpdateBovineChatSession(BovineChatSession session);
}
