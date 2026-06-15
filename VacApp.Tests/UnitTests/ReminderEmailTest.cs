using VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

namespace VacApp.Tests.UnitTests
{
    public class ReminderEmailTests
    {
        [Fact]
        public void Build_TenDays_NoExtraCollars_TotalIsBaseOnly()
        {
            var renewalUtc = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);

            var (subject, html) = ReminderEmail.Build(
                ReminderStage.TenDays, 149m, "PEN", renewalUtc, extraCollars: 0);

            Assert.Contains("10 días", subject);
            Assert.Contains("PEN 149.00", subject);
            Assert.Contains("PEN 149.00", html);
            Assert.Contains("01/07/2026", html);          // renewal in Peru time
            Assert.DoesNotContain("Collares adicionales", html);
        }

        [Fact]
        public void Build_FiveDays_WithCollars_TotalIncludesCollars()
        {
            // 2 extra collars → total 149 + 2*25 = 199.
            var renewalUtc = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);

            var (subject, html) = ReminderEmail.Build(
                ReminderStage.FiveDays, 199m, "PEN", renewalUtc, extraCollars: 2);

            Assert.Contains("5 días", subject);
            Assert.Contains("PEN 199.00", subject);
            Assert.Contains("Collares adicionales", html);
            Assert.Contains("2 ×", html);
            Assert.Contains("PEN 199.00", html);           // total line
        }
    }

    public class SubscriptionReminderTests
    {
        [Fact]
        public void ActivatePlus_LeavesBothRemindersPending()
        {
            var sub = new Subscription(userId: 1);
            sub.ActivatePlus();

            Assert.True(sub.NeedsReminder(ReminderStage.TenDays));
            Assert.True(sub.NeedsReminder(ReminderStage.FiveDays));
        }

        [Fact]
        public void MarkReminderSent_TenDays_DoesNotAffectFiveDays()
        {
            var sub = new Subscription(1);
            sub.ActivatePlus();

            sub.MarkReminderSent(ReminderStage.TenDays);

            Assert.False(sub.NeedsReminder(ReminderStage.TenDays));   // stamped for this cycle
            Assert.True(sub.NeedsReminder(ReminderStage.FiveDays));   // still pending
        }

        [Fact]
        public void ActivatePlus_AfterRenewal_ResetsReminders()
        {
            var sub = new Subscription(1);
            sub.ActivatePlus();
            sub.MarkReminderSent(ReminderStage.TenDays);
            sub.MarkReminderSent(ReminderStage.FiveDays);

            sub.ActivatePlus(); // new billing cycle

            Assert.True(sub.NeedsReminder(ReminderStage.TenDays));
            Assert.True(sub.NeedsReminder(ReminderStage.FiveDays));
        }
    }
}
