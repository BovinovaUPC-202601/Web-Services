using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;

namespace VacApp_Bovinova_Platform.AIAssistant.Domain.Services;

public interface IAIVisionService
{
    Task<BovineAnalysis> AnalyzeBovinePhotoAsync(
        int userId,
        int bovineId,
        string bovineContext,
        string imageBase64);
}
