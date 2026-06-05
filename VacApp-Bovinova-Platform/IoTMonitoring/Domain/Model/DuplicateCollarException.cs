namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model;

/// <summary>Thrown when a collar DeviceId is already registered (maps to HTTP 409).</summary>
public class DuplicateCollarException(string deviceId)
    : Exception($"El collar '{deviceId}' ya está registrado en el sistema.");
