using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;

public class BovineAnalysis
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int BovineId { get; private set; }
    public decimal Score { get; private set; }
    public string VisibleIssues { get; private set; }
    public UrgencyLevel UrgencyLevel { get; private set; }
    public string Recommendation { get; private set; }
    public decimal Confidence { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private BovineAnalysis()
    {
        VisibleIssues = string.Empty;
        Recommendation = string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    public BovineAnalysis(
        int userId,
        int bovineId,
        decimal score,
        string visibleIssues,
        UrgencyLevel urgencyLevel,
        string recommendation,
        decimal confidence)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId must be greater than 0", nameof(userId));

        if (bovineId <= 0)
            throw new ArgumentException("BovineId must be greater than 0", nameof(bovineId));

        if (score < 0)
            throw new ArgumentException("Score must not be negative", nameof(score));

        if (confidence < 0 || confidence > 1)
            throw new ArgumentException("Confidence must be between 0 and 1", nameof(confidence));

        UserId = userId;
        BovineId = bovineId;
        Score = score;
        VisibleIssues = visibleIssues;
        UrgencyLevel = urgencyLevel;
        Recommendation = recommendation;
        Confidence = confidence;
        CreatedAt = DateTime.UtcNow;
    }
}
