using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.ValueObjects;

namespace VacApp.Tests.UnitTests
{
    public class CollarTests
    {
        private static Collar NewCollar()
            => new(new RegisterCollarCommand(UserId: 1, DeviceId: "ESP32-AA", BovineId: 42));

        [Fact]
        public void RegisterCollar_IsActiveAndAssignedToBovine()
        {
            var collar = NewCollar();

            Assert.Equal("ESP32-AA", collar.DeviceId);
            Assert.Equal(42, collar.BovineId);
            Assert.Equal(CollarLifecycleStatus.Active, collar.LifecycleStatus);
            Assert.True(collar.IsActive);
        }

        [Fact]
        public void OperationalStatus_NoSignal_WhenNoRecentReading()
        {
            var collar = NewCollar();

            Assert.Equal(CollarOperationalStatus.NoSignal, collar.ResolveOperationalStatus(null));
            Assert.Equal(CollarOperationalStatus.NoSignal,
                collar.ResolveOperationalStatus(DateTime.UtcNow.AddMinutes(-30)));
        }

        [Fact]
        public void OperationalStatus_Active_WhenRecentReading()
        {
            var collar = NewCollar();

            Assert.Equal(CollarOperationalStatus.Active,
                collar.ResolveOperationalStatus(DateTime.UtcNow.AddMinutes(-2)));
        }

        [Fact]
        public void Maintenance_OverridesOperationalStatus()
        {
            var collar = NewCollar();
            collar.MarkMaintenance();

            Assert.False(collar.IsActive);
            Assert.Equal(CollarOperationalStatus.Maintenance,
                collar.ResolveOperationalStatus(DateTime.UtcNow));
        }

        [Fact]
        public void Suspend_OverridesOperationalStatus()
        {
            var collar = NewCollar();
            collar.Suspend();

            Assert.False(collar.IsActive);
            Assert.Equal(CollarOperationalStatus.Suspended,
                collar.ResolveOperationalStatus(DateTime.UtcNow));
        }
    }
}
