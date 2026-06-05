namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.ValueObjects;

/// <summary>
/// Administrative/lifecycle state of a collar (TP). Distinct from the runtime
/// operational status (ACTIVE/NO_SIGNAL) which is derived from telemetry timing.
/// </summary>
public enum CollarLifecycleStatus
{
    Active,
    Maintenance,
    Suspended
}
