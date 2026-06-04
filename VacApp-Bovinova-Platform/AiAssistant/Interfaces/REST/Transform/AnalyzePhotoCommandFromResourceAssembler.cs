using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Transform;

public static class AnalyzePhotoCommandFromResourceAssembler
{
    public static AnalyzePhotoCommand ToCommandFromResource(AnalyzePhotoResource resource, int userId)
    {
        return new AnalyzePhotoCommand(userId, resource.BovineId, resource.ImageBase64);
    }
}
