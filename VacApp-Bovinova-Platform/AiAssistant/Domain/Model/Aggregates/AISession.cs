using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Aggregates;

public class AISession
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int? BovineId { get; private set; }
    public SessionType SessionType { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private AISession()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public AISession(SendGeneralChatCommand command)
    {
        if (command.UserId <= 0)
            throw new ArgumentException("UserId must be greater than 0", nameof(command.UserId));

        UserId = command.UserId;
        SessionType = SessionType.General;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public AISession(SendBovineChatCommand command)
    {
        if (command.UserId <= 0)
            throw new ArgumentException("UserId must be greater than 0", nameof(command.UserId));

        if (command.BovineId <= 0)
            throw new ArgumentException("BovineId must be greater than 0", nameof(command.BovineId));

        UserId = command.UserId;
        BovineId = command.BovineId;
        SessionType = SessionType.Bovine;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
