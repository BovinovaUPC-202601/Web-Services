using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;

/// <summary>
/// A physical collar (ESP32 device) registered by a rancher and assigned to one bovine.
/// The DeviceId is the hardware identifier the collar publishes in its MQTT telemetry.
/// </summary>
public class Collar
{
    /// <summary>Minutes without telemetry after which a collar is considered NO_SIGNAL (TP).</summary>
    public const int SignalWindowMinutes = 10;

    public int                   Id              { get; private set; }
    public string                DeviceId        { get; private set; }
    public int                   BovineId        { get; private set; }
    public int                   UserId          { get; private set; }
    public DateTime              RegisteredAt    { get; private set; }
    public CollarLifecycleStatus LifecycleStatus { get; private set; }

    protected Collar()
    {
        DeviceId = string.Empty;
    }

    public Collar(RegisterCollarCommand command)
    {
        DeviceId        = command.DeviceId;
        BovineId        = command.BovineId;
        UserId          = command.UserId;
        RegisteredAt    = DateTime.UtcNow;
        LifecycleStatus = CollarLifecycleStatus.Active;
    }

    /// <summary>Counts toward billing and capacity.</summary>
    public bool IsActive => LifecycleStatus == CollarLifecycleStatus.Active;

    public void MarkMaintenance() => LifecycleStatus = CollarLifecycleStatus.Maintenance;
    public void Suspend()         => LifecycleStatus = CollarLifecycleStatus.Suspended;
    public void Reactivate()      => LifecycleStatus = CollarLifecycleStatus.Active;

    /// <summary>Reassigns the physical collar to a different bovine.</summary>
    public void Reassign(int bovineId) => BovineId = bovineId;

    /// <summary>
    /// Resolves the runtime operational status. Lifecycle overrides take precedence;
    /// otherwise ACTIVE if a reading arrived within the signal window, else NO_SIGNAL.
    /// </summary>
    public CollarOperationalStatus ResolveOperationalStatus(DateTime? lastSeenAtUtc)
    {
        if (LifecycleStatus == CollarLifecycleStatus.Maintenance) return CollarOperationalStatus.Maintenance;
        if (LifecycleStatus == CollarLifecycleStatus.Suspended)   return CollarOperationalStatus.Suspended;

        if (lastSeenAtUtc is null) return CollarOperationalStatus.NoSignal;
        return DateTime.UtcNow - lastSeenAtUtc.Value <= TimeSpan.FromMinutes(SignalWindowMinutes)
            ? CollarOperationalStatus.Active
            : CollarOperationalStatus.NoSignal;
    }
}
