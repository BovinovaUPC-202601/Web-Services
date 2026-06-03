namespace VacApp_Bovinova_Platform.IoTMonitoring.Infrastructure.Messaging;

/// <summary>
/// Configuration for the MQTT connection used to ingest IoT telemetry.
/// Values come from environment variables (see Program.cs).
/// </summary>
public class MqttSettings
{
    public string Host                = "localhost";
    public int    Port                = 1883;
    public string Username            = string.Empty; // empty => anonymous connection
    public string Password            = string.Empty;
    public string ClientId            = "vacapp-backend";
    public string TelemetryTopic      = "vacapp/telemetry";
    public string ResponseTopicPrefix = "vacapp/telemetry/response";

    // TLS — required when the broker listens on 8883 (e.g. Mosquitto on an Azure VM).
    public bool UseTls                    = false;
    // Allow self-signed / untrusted certificates (dev only). Leave false in production
    // when the broker uses a CA-issued certificate (e.g. Let's Encrypt).
    public bool AllowUntrustedCertificates = false;

    public bool HasCredentials => !string.IsNullOrWhiteSpace(Username);
}
