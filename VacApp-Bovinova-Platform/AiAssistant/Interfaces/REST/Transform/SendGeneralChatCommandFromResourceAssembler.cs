using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Transform;

public static class SendGeneralChatCommandFromResourceAssembler
{
    public static SendGeneralChatCommand ToCommandFromResource(GeneralChatMessageResource resource, int userId)
    {
        return new SendGeneralChatCommand(userId, resource.Message);
    }
}
