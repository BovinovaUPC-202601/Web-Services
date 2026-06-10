using MediatR;
using Moq;
using VacApp_Bovinova_Platform.IoTMonitoring.Application.Internal.CommandServices;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Events;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Repositories;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp.Tests.UnitTests
{
    public class BovineHealthRecordCommandServiceTests
    {
        private static Bovine BovineWithMaxTemp(double maxTemp) =>
            new("Ana", "female", new DateOnly(2020, 1, 1), "Holstein", 1, "img", userId: 42,
                minTemperature: 38.0, maxTemperature: maxTemp, minHeartRate: 40, maxHeartRate: 80);

        private static (BovineHealthRecordCommandService svc, Mock<IMediator> mediator) Build(Bovine? bovine)
        {
            var repo = new Mock<IBovineHealthRecordRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates.BovineHealthRecord>()))
                .Returns(Task.CompletedTask);
            var uow = new Mock<IUnitOfWork>();
            uow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
            var mediator = new Mock<IMediator>();
            var bovineQuery = new Mock<IBovineQueryService>();
            bovineQuery.Setup(q => q.Handle(It.IsAny<GetBovinesByIdQuery>())).ReturnsAsync(bovine!);

            var svc = new BovineHealthRecordCommandService(
                repo.Object, uow.Object, mediator.Object, bovineQuery.Object);
            return (svc, mediator);
        }

        [Fact]
        public async Task Handle_WithinBovineThresholds_DoesNotPublishAlert()
        {
            var (svc, mediator) = Build(BovineWithMaxTemp(40.0));
            // 39.8 is above the global max (39.5) but within this bovine's max (40).
            var command = new CreateBovineHealthRecordCommand(1, 42, "esp32", 39.8f, 60f, 90);

            var record = await svc.Handle(command);

            Assert.NotNull(record);
            Assert.False(record!.IsAlert);
            mediator.Verify(m => m.Publish(
                It.IsAny<AbnormalTelemetryDetectedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_AboveBovineMax_PublishesEventCarryingThresholds()
        {
            var (svc, mediator) = Build(BovineWithMaxTemp(40.0));
            var command = new CreateBovineHealthRecordCommand(1, 42, "esp32", 40.5f, 60f, 90);

            var record = await svc.Handle(command);

            Assert.NotNull(record);
            Assert.True(record!.IsAlert);
            mediator.Verify(m => m.Publish(
                It.Is<AbnormalTelemetryDetectedEvent>(e =>
                    e.BovineId == 1 &&
                    e.Temperature == 40.5f &&
                    e.MaxTemperature == 40f),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_BovineNotFound_FallsBackToGlobalRange()
        {
            var (svc, mediator) = Build(bovine: null);
            // 41°C is out of range under the global default (max 39.5) → alert.
            var command = new CreateBovineHealthRecordCommand(1, 42, "esp32", 41.0f, 60f, 90);

            var record = await svc.Handle(command);

            Assert.True(record!.IsAlert);
            mediator.Verify(m => m.Publish(
                It.IsAny<AbnormalTelemetryDetectedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
