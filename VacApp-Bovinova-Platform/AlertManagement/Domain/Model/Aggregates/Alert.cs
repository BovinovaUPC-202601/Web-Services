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
    public int          BovineId      { get; private set; }
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

    public void MarkAsRead()
    {
        Status = AlertStatus.Read;
    }
}
