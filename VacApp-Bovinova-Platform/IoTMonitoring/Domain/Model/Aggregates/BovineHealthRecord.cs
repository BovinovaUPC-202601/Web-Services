using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;

/// <summary>
/// Aggregate root that represents a single telemetry reading
/// from an ESP32 device monitoring a bovine's vital signs.
/// </summary>
public class BovineHealthRecord
{
    // Normal bovine ranges
    private const float MinTemperature = 38.0f;
    private const float MaxTemperature = 39.5f;
    private const float MinHeartRate   = 40.0f;
    private const float MaxHeartRate   = 80.0f;

    public int      Id          { get; private set; }
    public int      BovineId    { get; private set; }
    public int      UserId      { get; private set; }   // owner (rancher) of the bovine
    public string   DeviceId    { get; private set; }
    public float    Temperature { get; private set; }   // °C
    public float    HeartRate   { get; private set; }   // bpm
    public bool     IsAlert     { get; private set; }
    public DateTime RecordedAt  { get; private set; }

    protected BovineHealthRecord()
    {
        DeviceId = string.Empty;
    }

    public BovineHealthRecord(CreateBovineHealthRecordCommand command)
    {
        BovineId    = command.BovineId;
        UserId      = command.UserId;
        DeviceId    = command.DeviceId;
        Temperature = command.Temperature;
        HeartRate   = command.HeartRate;
        RecordedAt  = DateTime.UtcNow;
        IsAlert     = EvaluateAlert(command.Temperature, command.HeartRate);
    }

    /// <summary>
    /// Returns true when any vital sign is outside the normal bovine range.
    /// </summary>
    public static bool EvaluateAlert(float temperature, float heartRate)
    {
        return temperature < MinTemperature
            || temperature > MaxTemperature
            || heartRate   < MinHeartRate
            || heartRate   > MaxHeartRate;
    }
}
