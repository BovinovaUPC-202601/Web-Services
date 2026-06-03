namespace VacApp_Bovinova_Platform.Shared.Domain.Model;

/// <summary>
/// Shared Kernel: normal physiological ranges for bovine vital signs.
/// Single source of truth shared by IoTMonitoring (telemetry evaluation)
/// and AlertManagement (urgency + message wording).
/// </summary>
public static class BovineVitalRanges
{
    public const float MinTemperature = 38.0f; // °C
    public const float MaxTemperature = 39.5f; // °C
    public const float MinHeartRate   = 40.0f; // bpm
    public const float MaxHeartRate   = 80.0f; // bpm

    public static bool IsTemperatureOutOfRange(float temperature)
        => temperature < MinTemperature || temperature > MaxTemperature;

    public static bool IsHeartRateOutOfRange(float heartRate)
        => heartRate < MinHeartRate || heartRate > MaxHeartRate;
}
