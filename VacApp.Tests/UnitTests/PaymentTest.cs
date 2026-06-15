using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

namespace VacApp.Tests.UnitTests
{
    public class PaymentTests
    {
        [Fact]
        public void NewPayment_StartsPendingInSoles()
        {
            var payment = new Payment(userId: 1, PaymentConcept.PlusMonthly, amount: 149m, idempotencyKey: "key-1");

            Assert.Equal(PaymentStatus.Pending, payment.Status);
            Assert.Equal("PEN", payment.Currency);
            Assert.Equal(149m, payment.Amount);
            Assert.Equal("key-1", payment.IdempotencyKey);
            Assert.Null(payment.PaidAt);
            Assert.Null(payment.ProviderRef);
        }

        [Fact]
        public void MarkPaid_FromPending_SetsPaidWithRefAndTimestamp()
        {
            var payment = new Payment(1, PaymentConcept.PlusMonthly, 149m, "key-1");

            payment.MarkPaid("cs_test_123");

            Assert.Equal(PaymentStatus.Paid, payment.Status);
            Assert.Equal("cs_test_123", payment.ProviderRef);
            Assert.NotNull(payment.PaidAt);
        }

        [Fact]
        public void MarkPaid_Twice_IsIdempotent()
        {
            // The gateway may confirm twice; the second confirmation must not move state again.
            var payment = new Payment(1, PaymentConcept.PlusMonthly, 149m, "key-1");
            payment.MarkPaid("cs_test_123");
            var firstPaidAt = payment.PaidAt;

            payment.MarkPaid("cs_test_999");

            Assert.Equal(PaymentStatus.Paid, payment.Status);
            Assert.Equal("cs_test_123", payment.ProviderRef);
            Assert.Equal(firstPaidAt, payment.PaidAt);
        }

        [Fact]
        public void MarkFailed_FromPending_SetsFailed()
        {
            var payment = new Payment(1, PaymentConcept.PlusMonthly, 149m, "key-1");

            payment.MarkFailed();

            Assert.Equal(PaymentStatus.Failed, payment.Status);
            Assert.Null(payment.PaidAt);
        }

        [Fact]
        public void MarkFailed_AfterPaid_IsNoOp()
        {
            var payment = new Payment(1, PaymentConcept.PlusMonthly, 149m, "key-1");
            payment.MarkPaid("cs_test_123");

            payment.MarkFailed();

            Assert.Equal(PaymentStatus.Paid, payment.Status);
        }

        [Fact]
        public void Refund_FromPaid_SetsRefunded()
        {
            var payment = new Payment(1, PaymentConcept.PlusMonthly, 149m, "key-1");
            payment.MarkPaid("cs_test_123");

            payment.Refund();

            Assert.Equal(PaymentStatus.Refunded, payment.Status);
        }

        [Fact]
        public void Refund_FromPending_IsNoOp()
        {
            var payment = new Payment(1, PaymentConcept.PlusMonthly, 149m, "key-1");

            payment.Refund();

            Assert.Equal(PaymentStatus.Pending, payment.Status);
        }

        [Fact]
        public void LinkProviderRef_KeepsPendingButStoresRef()
        {
            // Set when the checkout session is created, before payment confirms.
            var payment = new Payment(1, PaymentConcept.AdditionalCollar, 25m, "key-2");

            payment.LinkProviderRef("cs_test_abc");

            Assert.Equal(PaymentStatus.Pending, payment.Status);
            Assert.Equal("cs_test_abc", payment.ProviderRef);
        }
    }
}
