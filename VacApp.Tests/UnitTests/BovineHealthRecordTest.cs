using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;

namespace VacApp.Tests.UnitTests
{
    public class BovineHealthRecordTests
    {
        // Per-bovine thresholds: maxTemp = 40 (above the global default 39.5),
        // so these tests prove the bovine's own range is used, not the global one.
        private const float MinTemp = 38f;
        private const float MaxTemp = 40f;
        private const float MinHr = 40f;
        private const float MaxHr = 80f;

        private static BovineHealthRecord Record(float temp, float hr) =>
            new(new CreateBovineHealthRecordCommand(
                BovineId: 1, UserId: 42, DeviceId: "esp32-001",
                Temperature: temp, HeartRate: hr, BatteryLevel: 90));

        [Fact]
        public void CreateRecord_PersistsFields_AndDefaultsToNoAlert()
        {
            var record = Record(38.5f, 60f);

            Assert.Equal(1, record.BovineId);
            Assert.Equal(42, record.UserId);
            Assert.Equal(90, record.BatteryLevel);
            // IsAlert is not decided at construction — it needs the bovine thresholds.
            Assert.False(record.IsAlert);
        }

        [Fact]
        public void EvaluateAlert_WithinBovineRange_NoAlert_EvenIfAboveGlobalMax()
        {
            // 39.8°C is above the GLOBAL max (39.5) but below this bovine's max (40).
            var record = Record(39.8f, 60f);

            Assert.False(record.EvaluateAlert(MinTemp, MaxTemp, MinHr, MaxHr));
            Assert.False(record.IsAlert);
        }

        [Fact]
        public void EvaluateAlert_AboveBovineMax_RaisesAlert()
        {
            var record = Record(40.5f, 60f);

            Assert.True(record.EvaluateAlert(MinTemp, MaxTemp, MinHr, MaxHr));
            Assert.True(record.IsAlert);
        }

        [Theory]
        [InlineData(38.0f, 40f, false)]   // both at lower limit → normal (inclusive)
        [InlineData(40.0f, 80f, false)]   // both at custom upper limit → normal
        [InlineData(37.9f, 60f, true)]    // temp below min
        [InlineData(38.5f, 39f, true)]    // bpm below min
        [InlineData(40.1f, 60f, true)]    // temp above custom max
        [InlineData(38.5f, 81f, true)]    // bpm above max
        public void EvaluateAlert_RespectsBovineRanges(float temp, float bpm, bool expected)
        {
            var record = Record(temp, bpm);

            Assert.Equal(expected, record.EvaluateAlert(MinTemp, MaxTemp, MinHr, MaxHr));
        }
    }
}
