namespace VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Resources;

public record BovineAnalysisResource(
    int Id,
    int BovineId,
    decimal Score,
    string VisibleIssues,
    string Urgency,
    string Recommendation,
    decimal Confidence,
    DateTime CreatedAt);
