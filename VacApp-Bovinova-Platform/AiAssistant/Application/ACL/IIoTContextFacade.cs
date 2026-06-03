namespace VacApp_Bovinova_Platform.AIAssistant.Application.ACL;

/// <summary>
/// Anti-corruption layer that exposes IoT telemetry from the IoTMonitoring
/// bounded context as plain-text context for the AI assistant prompts.
/// </summary>
public interface IIoTContextFacade
{
    Task<string> GetBovineTelemetryContextAsync(int bovineId);
}
