namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;

// DeviceId is the globally-unique hardware id the app supplies (e.g. collar-2-a3f9c1);
// it is returned in the response so the rancher can flash that exact value into the
// ESP32. The collar's bovine/owner binding is set server-side (never spoofable).
public record RegisterCollarCommand(int UserId, string DeviceId, int BovineId);
