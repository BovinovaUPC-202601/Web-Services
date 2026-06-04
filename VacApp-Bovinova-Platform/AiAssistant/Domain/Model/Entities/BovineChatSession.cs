using System.Text.Json;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;

public class BovineChatSession
{
    private const int MaxMessages = 20;

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int BovineId { get; private set; }
    public string MessagesJson { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private BovineChatSession()
    {
        MessagesJson = "[]";
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public BovineChatSession(int userId, int bovineId)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId must be greater than 0", nameof(userId));

        if (bovineId <= 0)
            throw new ArgumentException("BovineId must be greater than 0", nameof(bovineId));

        UserId = userId;
        BovineId = bovineId;
        MessagesJson = "[]";
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public IReadOnlyCollection<ChatMessage> GetMessages()
    {
        return DeserializeMessages().ToList();
    }

    public void AddMessage(ChatMessage message)
    {
        var messages = DeserializeMessages().ToList();
        messages.Add(message);
        MessagesJson = JsonSerializer.Serialize(messages.TakeLast(MaxMessages));
        UpdatedAt = DateTime.UtcNow;
    }

    private IEnumerable<ChatMessage> DeserializeMessages()
    {
        return JsonSerializer.Deserialize<List<ChatMessage>>(MessagesJson) ?? new List<ChatMessage>();
    }
}
