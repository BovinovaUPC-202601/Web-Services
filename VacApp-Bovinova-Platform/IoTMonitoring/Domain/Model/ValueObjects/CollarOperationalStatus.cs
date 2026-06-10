namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.ValueObjects;

/// <summary>
/// Runtime status derived from telemetry timing (TP): ACTIVE if the collar sent a
/// valid reading within the signal window, NO_SIGNAL otherwise. MAINTENANCE / SUSPENDED
/// reflect the lifecycle state when the collar is not operationally active.
/// </summary>
public enum CollarOperationalStatus
{
    Active,
    NoSignal,
    Maintenance,
    Suspended
}
