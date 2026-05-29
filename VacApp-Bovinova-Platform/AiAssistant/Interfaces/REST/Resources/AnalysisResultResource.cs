namespace VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Resources;

public record AnalysisResultResource(
    decimal Score,
    string VisibleIssues,
    string Urgency,
    string Recommendation,
    decimal Confidence);
