using MediatR;
using Moq;
using VacApp_Bovinova_Platform.AlertManagement.Application.EventHandlers;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Services;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Events;

namespace VacApp.Tests.UnitTests
{
    public class CollarReturnAlertTests
    {
        [Fact]
        public void ForCollarReturn_IsAccountLevel_WithCount()
        {
            var alert = Alert.ForCollarReturn(userId: 7, collarCount: 3);

            Assert.Null(alert.BovineId);                       // account-level, no bovine
            Assert.Equal(7, alert.UserId);
            Assert.Equal(AlertType.CollarReturn, alert.AlertType);
            Assert.Equal(AlertStatus.Unread, alert.Status);
            Assert.Contains("3 collares", alert.Message);
        }

        [Fact]
        public void ForCollarReturn_SingularMessage_ForOneCollar()
        {
            var alert = Alert.ForCollarReturn(1, 1);
            Assert.Contains("1 collar IoT", alert.Message);
            Assert.DoesNotContain("collares", alert.Message);
        }

        [Fact]
        public async Task Handler_RaisesCollarReturnAlert_FromEvent()
        {
            var commandService = new Mock<IAlertCommandService>();
            var handler = new SubscriptionEndedHandler(commandService.Object);

            await handler.Handle(new SubscriptionEndedEvent(UserId: 42, CollarCount: 2), CancellationToken.None);

            commandService.Verify(s => s.Handle(
                It.Is<RegisterCollarReturnAlertCommand>(c => c.UserId == 42 && c.CollarCount == 2)),
                Times.Once);
        }

        [Fact]
        public void SubscriptionEndedEvent_IsMediatRNotification()
        {
            // Guards the cross-BC contract: the event must be an INotification to be published/handled.
            Assert.IsAssignableFrom<INotification>(new SubscriptionEndedEvent(1, 0));
        }
    }
}
