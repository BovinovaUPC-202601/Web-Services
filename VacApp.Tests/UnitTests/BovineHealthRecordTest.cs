using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;

namespace VacApp.Tests.UnitTests
{
    public class BovineHealthRecordTests
    {
        [Fact]
        public void CreateRecord_PersistsUserId()
        {
            // Arrange — ESP32 payload carries the owning rancher's id
            var command = new CreateBovineHealthRecordCommand(
                BovineId: 1, UserId: 42, DeviceId: "esp32-001",
                Temperature: 38.5f, HeartRate: 60f, BatteryLevel: 90);

            // Act
            var record = new BovineHealthRecord(command);

            // Assert
            Assert.Equal(1, record.BovineId);
            Assert.Equal(42, record.UserId);
            Assert.Equal(90, record.BatteryLevel);
            Assert.False(record.IsAlert);
        }

        [Fact]
        public void CreateRecord_OutOfRange_RaisesAlert()
        {
            var command = new CreateBovineHealthRecordCommand(
                BovineId: 1, UserId: 42, DeviceId: "esp32-001",
                Temperature: 41.0f, HeartRate: 120f, BatteryLevel: 75);

            var record = new BovineHealthRecord(command);

            Assert.True(record.IsAlert);
        }

        [Theory]
        [InlineData(38.0f, 40f, false)]   // limits inclusive → normal
        [InlineData(39.5f, 80f, false)]
        [InlineData(37.9f, 60f, true)]    // temp too low
        [InlineData(38.5f, 39f, true)]    // bpm too low
        public void EvaluateAlert_RespectsBovineRanges(float temp, float bpm, bool expected)
        {
            Assert.Equal(expected, BovineHealthRecord.EvaluateAlert(temp, bpm));
        }
    }
}
