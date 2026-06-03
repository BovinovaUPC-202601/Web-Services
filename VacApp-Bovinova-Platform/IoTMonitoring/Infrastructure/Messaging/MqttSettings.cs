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

    public bool HasCredentials => !string.IsNullOrWhiteSpace(Username);
}
