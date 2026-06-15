using System.Globalization;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;

/// <summary>
/// Builds the purchase-receipt email content (subject + HTML) from a confirmed payment.
/// Pure formatting — no I/O — so it stays trivially testable and provider-agnostic.
/// </summary>
public static class ReceiptEmail
{
    /// <summary>Peru time (UTC-5, no DST). Resolves the IANA id on Linux/macOS and the
    /// Windows id, falling back to a fixed -5 offset if neither is present.</summary>
    private static readonly TimeZoneInfo PeruTimeZone = ResolvePeruTimeZone();

    private static TimeZoneInfo ResolvePeruTimeZone()
    {
        foreach (var id in new[] { "America/Lima", "SA Pacific Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.CreateCustomTimeZone("Peru", TimeSpan.FromHours(-5), "Peru", "Peru");
    }

    public static (string Subject, string Html) Build(
        PaymentConcept concept, decimal amount, string currency, DateTime paidAtUtc)
    {
        var conceptLabel = concept == PaymentConcept.PlusMonthly
            ? "Plan Plus (mensual)"
            : "Collar adicional (mensual)";

        // PaidAt is stored in UTC; show it in Peru local time (UTC-5, no DST) so the
        // receipt matches the buyer's clock instead of being 5 hours ahead.
        var paidAtPeru = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(paidAtUtc, DateTimeKind.Utc), PeruTimeZone);

        var money = $"{currency} {amount.ToString("0.00", CultureInfo.InvariantCulture)}";
        var date  = paidAtPeru.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) + " (hora Perú)";
        var nextRenewal = paidAtPeru.AddMonths(1).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        var subject = $"VacApp — Factura de tu compra ({conceptLabel})";

        var html = $"""
            <div style="font-family:Arial,Helvetica,sans-serif;max-width:520px;margin:0 auto;color:#1a1a1a">
              <h2 style="color:#2e7d32;margin-bottom:4px">VacApp · Bovinova</h2>
              <p style="margin-top:0;color:#555">Gracias por tu compra. Aquí está tu factura.</p>
              <table style="width:100%;border-collapse:collapse;margin:16px 0">
                <tr><td style="padding:8px 0;color:#777">Concepto</td><td style="padding:8px 0;text-align:right;font-weight:bold">{conceptLabel}</td></tr>
                <tr><td style="padding:8px 0;color:#777">Monto</td><td style="padding:8px 0;text-align:right;font-weight:bold">{money}</td></tr>
                <tr><td style="padding:8px 0;color:#777">Fecha de pago</td><td style="padding:8px 0;text-align:right">{date}</td></tr>
                <tr><td style="padding:8px 0;color:#777">Próxima renovación</td><td style="padding:8px 0;text-align:right">{nextRenewal}</td></tr>
              </table>
              <p style="color:#999;font-size:12px">Pago simulado con fines de demostración. VacApp no realiza cobros reales.</p>
            </div>
            """;

        return (subject, html);
    }
}
