using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Aggregates;

/// <summary>
/// Aggregate root for a bovine health alert.
/// Created when IoTMonitoring detects abnormal telemetry (FEVER)
/// or AIAssistant finds a high-urgency visual anomaly (VISUAL_ANOMALY).
/// </summary>
public class Alert
{
    public int          Id            { get; private set; }
    /// <summary>The bovine this alert is about. Null for account-level alerts
    /// (e.g. <see cref="AlertType.CollarReturn"/>), which are not tied to one animal.</summary>
    public int?         BovineId      { get; private set; }
    public int          UserId        { get; private set; }
    public AlertType    AlertType     { get; private set; }
    public UrgencyLevel UrgencyLevel  { get; private set; }
    public AlertStatus  Status        { get; private set; }
    public string       Message       { get; private set; }
    public DateTime     CreatedAt     { get; private set; }

    protected Alert() { Message = string.Empty; }

    public Alert(RegisterAlertCommand command)
    {
        BovineId     = command.BovineId;
        UserId       = command.UserId;
        AlertType    = command.AlertType;
        UrgencyLevel = command.UrgencyLevel;
        Message      = command.Message;
        Status       = AlertStatus.Unread;
        CreatedAt    = DateTime.UtcNow;
    }

    /// <summary>Private ctor for account-level alerts (no bovine).</summary>
    private Alert(int userId, AlertType alertType, UrgencyLevel urgency, string message)
    {
        BovineId     = null;
        UserId       = userId;
        AlertType    = alertType;
        UrgencyLevel = urgency;
        Message      = message;
        Status       = AlertStatus.Unread;
        CreatedAt    = DateTime.UtcNow;
    }

    /// <summary>
    /// Account-level alert telling the user to return their IoT collars after the Plus
    /// plan ended (suspension). Not tied to a bovine.
    /// </summary>
    public static Alert ForCollarReturn(int userId, int collarCount)
    {
        var message = collarCount > 0
            ? $"Tu plan Plus terminó. Debés devolver {collarCount} collar{(collarCount == 1 ? "" : "es")} IoT."
            : "Tu plan Plus terminó. Debés devolver tus collares IoT.";
        return new Alert(userId, AlertType.CollarReturn, UrgencyLevel.Red, message);
    }

    public void MarkAsRead()
    {
        Status = AlertStatus.Read;
    }
}
