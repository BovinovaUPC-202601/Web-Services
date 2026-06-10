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

    // Per-bovine evaluation: the rancher configures custom thresholds on each
    // bovine; telemetry must be judged against those, not the shared defaults.
    public static bool IsTemperatureOutOfRange(float temperature, float min, float max)
        => temperature < min || temperature > max;

    public static bool IsHeartRateOutOfRange(float heartRate, float min, float max)
        => heartRate < min || heartRate > max;

    // Effective threshold: use the configured per-bovine value, or fall back to
    // the shared default when it's unset (0 — e.g. bovines created before the
    // threshold columns existed).
    public static float Resolve(double configured, float fallback)
        => configured > 0 ? (float)configured : fallback;
}
