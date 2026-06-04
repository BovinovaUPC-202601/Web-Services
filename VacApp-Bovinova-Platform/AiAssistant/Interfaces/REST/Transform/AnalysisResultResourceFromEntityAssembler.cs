using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;
using VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Transform;

public static class AnalysisResultResourceFromEntityAssembler
{
    public static AnalysisResultResource ToResourceFromEntity(BovineAnalysis entity)
    {
        return new AnalysisResultResource(
            entity.Score,
            entity.VisibleIssues,
            entity.UrgencyLevel.ToString().ToUpperInvariant(),
            entity.Recommendation,
            entity.Confidence);
    }
}
