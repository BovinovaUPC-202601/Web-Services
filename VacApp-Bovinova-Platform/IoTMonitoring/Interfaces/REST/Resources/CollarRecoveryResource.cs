namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;

public record CollarRecoveryResource(
    string Rancher,
    string Contact,
    string DeviceId,
    DateTime? SuspendedAt,
    string SubscriptionStatus,
    string CollarStatus);
