using VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

namespace VacApp.Tests.UnitTests
{
    public class ReceiptEmailTests
    {
        [Fact]
        public void Build_PlusMonthly_HasConceptAmountAndDates()
        {
            var paidAt = new DateTime(2026, 6, 15, 10, 30, 0, DateTimeKind.Utc);

            var (subject, html) = ReceiptEmail.Build(
                PaymentConcept.PlusMonthly, 149m, "PEN", paidAt);

            Assert.Contains("Plan Plus", subject);
            Assert.Contains("Plan Plus (mensual)", html);
            Assert.Contains("PEN 149.00", html);
            Assert.Contains("15/06/2026", html);   // payment date
            Assert.Contains("15/07/2026", html);   // next renewal (+1 month)
        }

        [Fact]
        public void Build_ConvertsUtcToPeruTime_MinusFiveHours()
        {
            // 15:57 UTC must render as 10:57 Peru (UTC-5), matching the buyer's clock.
            var paidAtUtc = new DateTime(2026, 6, 15, 15, 57, 0, DateTimeKind.Utc);

            var (_, html) = ReceiptEmail.Build(
                PaymentConcept.PlusMonthly, 149m, "PEN", paidAtUtc);

            Assert.Contains("15/06/2026 10:57", html);
            Assert.Contains("hora Perú", html);
            Assert.DoesNotContain("15:57", html);
        }

        [Fact]
        public void Build_AdditionalCollar_LabelsCollarAndMonto()
        {
            var paidAt = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc);

            var (subject, html) = ReceiptEmail.Build(
                PaymentConcept.AdditionalCollar, 25m, "PEN", paidAt);

            Assert.Contains("Collar adicional", subject);
            Assert.Contains("Collar adicional (mensual)", html);
            Assert.Contains("PEN 25.00", html);
            Assert.Contains("28/02/2026", html);   // +1 month clamps to Feb
        }
    }
}
