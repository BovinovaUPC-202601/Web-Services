using VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

namespace VacApp.Tests.UnitTests
{
    public class SuspensionEmailTests
    {
        [Fact]
        public void Build_ShowsOverdueDateAndTotal_NoCollars()
        {
            var renewalUtc = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

            var (subject, html) = SuspensionEmail.Build(149m, "PEN", renewalUtc, extraCollars: 0);

            Assert.Contains("suspendida", subject);
            Assert.Contains("01/06/2026", html);       // overdue date in Peru time
            Assert.Contains("PEN 149.00", html);
            Assert.DoesNotContain("collares adicional", html);
        }

        [Fact]
        public void Build_WithCollars_TotalAndCollarNote()
        {
            var renewalUtc = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

            var (_, html) = SuspensionEmail.Build(199m, "PEN", renewalUtc, extraCollars: 2);

            Assert.Contains("PEN 199.00", html);
            Assert.Contains("2 collares adicionales", html);
        }
    }

    public class SubscriptionSuspendTests
    {
        [Fact]
        public void Suspend_PlusActive_BecomesSuspendedAndDropsFromActiveQuery()
        {
            var sub = new Subscription(userId: 1);
            sub.ActivatePlus();

            sub.Suspend();

            Assert.Equal(SubscriptionStatus.Suspended, sub.Status);
            Assert.NotNull(sub.SuspendedAt);
            Assert.False(sub.IsPlusActive); // no longer matched by the active-Plus sweep
        }

        [Fact]
        public void Suspend_Free_IsNoOp()
        {
            var sub = new Subscription(1); // Free by default

            sub.Suspend();

            Assert.Equal(SubscriptionStatus.Active, sub.Status);
        }
    }
}
