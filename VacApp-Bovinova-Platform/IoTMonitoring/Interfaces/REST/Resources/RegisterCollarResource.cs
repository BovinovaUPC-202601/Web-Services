namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;

// DeviceId is the globally-unique hardware id the app generates and shows to the
// rancher to flash into the ESP32. BovineId chooses the assignment.
public record RegisterCollarResource(string DeviceId, int BovineId);
