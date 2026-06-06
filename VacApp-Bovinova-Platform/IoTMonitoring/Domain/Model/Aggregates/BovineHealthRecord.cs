using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;
using VacApp_Bovinova_Platform.Shared.Domain.Model;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;

/// <summary>
/// Aggregate root that represents a single telemetry reading
/// from an ESP32 device monitoring a bovine's vital signs.
/// </summary>
public class BovineHealthRecord
{
    public int      Id           { get; private set; }
    public int      BovineId     { get; private set; }
    public int      UserId       { get; private set; }   // owner (rancher) of the bovine
    public string   DeviceId     { get; private set; }
    public float    Temperature  { get; private set; }   // °C
    public float    HeartRate    { get; private set; }   // bpm
    public int      BatteryLevel { get; private set; }   // collar battery charge, 0–100 %
    public bool     IsAlert      { get; private set; }
    public DateTime RecordedAt   { get; private set; }

    protected BovineHealthRecord()
    {
        DeviceId = string.Empty;
    }

    public BovineHealthRecord(CreateBovineHealthRecordCommand command)
    {
        BovineId     = command.BovineId;
        UserId       = command.UserId;
        DeviceId     = command.DeviceId;
        Temperature  = command.Temperature;
        HeartRate    = command.HeartRate;
        BatteryLevel = command.BatteryLevel;
        RecordedAt   = DateTime.UtcNow;
        // IsAlert stays false until evaluated against the bovine's own thresholds
        // (the reading alone doesn't know the configured range).
        IsAlert      = false;
    }

    /// <summary>
    /// Flags the record as an alert when a vital sign falls outside the bovine's
    /// configured thresholds. Limits are inclusive. Returns the resulting state.
    /// </summary>
    public bool EvaluateAlert(float minTemperature, float maxTemperature,
                              float minHeartRate, float maxHeartRate)
    {
        IsAlert = BovineVitalRanges.IsTemperatureOutOfRange(Temperature, minTemperature, maxTemperature)
               || BovineVitalRanges.IsHeartRateOutOfRange(HeartRate, minHeartRate, maxHeartRate);
        return IsAlert;
    }
}
