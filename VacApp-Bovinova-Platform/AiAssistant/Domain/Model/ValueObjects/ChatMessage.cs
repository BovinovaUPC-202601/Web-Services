namespace VacApp_Bovinova_Platform.AIAssistant.Domain.Model.ValueObjects;

public record ChatMessage
{
    public string Role { get; init; }
    public string Content { get; init; }
    public DateTime Timestamp { get; init; }

    private ChatMessage()
    {
        Role = string.Empty;
        Content = string.Empty;
        Timestamp = DateTime.UtcNow;
    }

    public ChatMessage(string role, string content, DateTime timestamp)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role must not be empty", nameof(role));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content must not be empty", nameof(content));

        Role = role;
        Content = content;
        Timestamp = timestamp;
    }
}
