using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Transform;

public static class SendBovineChatCommandFromResourceAssembler
{
    public static SendBovineChatCommand ToCommandFromResource(BovineChatMessageResource resource, int userId, int effectiveUserId)
    {
        return new SendBovineChatCommand(userId, effectiveUserId, resource.BovineId, resource.Message);
    }
}
