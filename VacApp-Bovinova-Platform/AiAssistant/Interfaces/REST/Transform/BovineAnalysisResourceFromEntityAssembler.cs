using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;
using VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Transform;

public static class BovineAnalysisResourceFromEntityAssembler
{
    public static BovineAnalysisResource ToResourceFromEntity(BovineAnalysis entity)
    {
        return new BovineAnalysisResource(
            entity.Id,
            entity.BovineId,
            entity.Score,
            entity.VisibleIssues,
            entity.UrgencyLevel.ToString().ToUpperInvariant(),
            entity.Recommendation,
            entity.Confidence,
            entity.CreatedAt);
    }
}
