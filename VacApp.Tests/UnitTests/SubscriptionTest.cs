using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

namespace VacApp.Tests.UnitTests
{
    public class SubscriptionTests
    {
        [Fact]
        public void NewSubscription_DefaultsToActiveFree()
        {
            var sub = new Subscription(userId: 1);

            Assert.Equal(PlanType.Free, sub.Plan);
            Assert.Equal(SubscriptionStatus.Active, sub.Status);
            Assert.False(sub.IsPlusActive);
        }

        [Fact]
        public void ActivatePlus_SetsPlusActiveWithRenewal()
        {
            var sub = new Subscription(userId: 1);

            sub.ActivatePlus();

            Assert.True(sub.IsPlusActive);
            Assert.NotNull(sub.StartDate);
            Assert.NotNull(sub.NextRenewal);
            Assert.True(sub.NextRenewal > sub.StartDate);
        }

        [Fact]
        public void Suspend_BlocksPlusAccess()
        {
            var sub = new Subscription(userId: 1);
            sub.ActivatePlus();

            sub.Suspend();

            Assert.Equal(SubscriptionStatus.Suspended, sub.Status);
            Assert.False(sub.IsPlusActive);
        }

        [Fact]
        public void MonthlyCost_BaseWhenWithinIncludedCollars()
        {
            // 149 base, 3 collars included → no extra
            Assert.Equal(149m, SubscriptionPricing.MonthlyCost(0));
            Assert.Equal(149m, SubscriptionPricing.MonthlyCost(3));
        }

        [Theory]
        [InlineData(4, 174)]  // 149 + 25*1
        [InlineData(5, 199)]  // 149 + 25*2
        [InlineData(6, 224)]  // 149 + 25*3
        public void MonthlyCost_AddsPerAdditionalActiveCollar(int activeCollars, decimal expected)
        {
            Assert.Equal(expected, SubscriptionPricing.MonthlyCost(activeCollars));
        }

        [Fact]
        public void Suspend_SetsSuspendedAtAndBlocksPlus()
        {
            var sub = new Subscription(userId: 1);
            sub.ActivatePlus();

            sub.Suspend();

            Assert.Equal(SubscriptionStatus.Suspended, sub.Status);
            Assert.NotNull(sub.SuspendedAt);
            Assert.False(sub.IsPlusActive);
        }

        [Fact]
        public void Reactivate_ClearsSuspension()
        {
            var sub = new Subscription(userId: 1);
            sub.ActivatePlus();
            sub.Suspend();

            sub.ActivatePlus();

            Assert.True(sub.IsPlusActive);
            Assert.Null(sub.SuspendedAt);
        }

        [Fact]
        public void AdditionalCollarRequest_StartsPendingThenApproved()
        {
            var request = new AdditionalCollarRequest(userId: 1, monthlyAmount: 25m);
            Assert.Equal(AdditionalCollarStatus.Requested, request.Status);
            Assert.False(request.IsActive);

            request.Approve();
            Assert.Equal(AdditionalCollarStatus.Approved, request.Status);
            Assert.True(request.IsActive);
        }
    }
}
