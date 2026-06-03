using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VacApp_Bovinova_Platform.AIAssistant.Infrastructure.Persistence.EFC.Repositories;

public class AISessionRepository(AppDbContext context)
    : BaseRepository<GeneralChatSession>(context), IAISessionRepository
{
    public async Task<GeneralChatSession?> FindGeneralChatSessionByUserIdAsync(int userId)
    {
        return await Context.Set<GeneralChatSession>()
            .FirstOrDefaultAsync(session => session.UserId == userId);
    }

    public async Task<BovineChatSession?> FindBovineChatSessionByUserIdAndBovineIdAsync(int userId, int bovineId)
    {
        return await Context.Set<BovineChatSession>()
            .FirstOrDefaultAsync(session => session.UserId == userId && session.BovineId == bovineId);
    }

    public async Task AddGeneralChatSessionAsync(GeneralChatSession session)
    {
        await Context.Set<GeneralChatSession>().AddAsync(session);
    }

    public async Task AddBovineChatSessionAsync(BovineChatSession session)
    {
        await Context.Set<BovineChatSession>().AddAsync(session);
    }

    public void UpdateGeneralChatSession(GeneralChatSession session)
    {
        Context.Set<GeneralChatSession>().Update(session);
    }

    public void UpdateBovineChatSession(BovineChatSession session)
    {
        Context.Set<BovineChatSession>().Update(session);
    }
}
