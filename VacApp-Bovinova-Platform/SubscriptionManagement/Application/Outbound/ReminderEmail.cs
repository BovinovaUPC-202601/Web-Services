using System.Globalization;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;

/// <summary>
/// Builds the pre-renewal payment reminder email (subject + HTML). Pure formatting —
/// no I/O. The amount is the full monthly total (Plus base + extra collars), already
/// computed by the caller.
/// </summary>
public static class ReminderEmail
{
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
        ReminderStage stage, decimal total, string currency,
        DateTime renewalUtc, int extraCollars)
    {
        var days = (int)stage; // 10 or 5
        var renewalPeru = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(renewalUtc, DateTimeKind.Utc), PeruTimeZone);
        var renewalDate = renewalPeru.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var money = $"{currency} {total.ToString("0.00", CultureInfo.InvariantCulture)}";

        var collarLine = extraCollars > 0
            ? $"<tr><td style=\"padding:8px 0;color:#777\">Collares adicionales</td><td style=\"padding:8px 0;text-align:right\">{extraCollars} × {currency} {SubscriptionPricingDisplay.Collar}</td></tr>"
            : string.Empty;

        var subject = $"VacApp — Tu plan vence en {days} días ({money})";

        var html = $"""
            <div style="font-family:Arial,Helvetica,sans-serif;max-width:520px;margin:0 auto;color:#1a1a1a">
              <h2 style="color:#2e7d32;margin-bottom:4px">VacApp · Bovinova</h2>
              <p style="margin-top:0">Tu plan <strong>Plus</strong> se renueva el <strong>{renewalDate}</strong> — faltan <strong>{days} días</strong>.</p>
              <p>Para no perder el acceso, recordá pagar el total del mes:</p>
              <table style="width:100%;border-collapse:collapse;margin:16px 0">
                <tr><td style="padding:8px 0;color:#777">Plan Plus (base)</td><td style="padding:8px 0;text-align:right">{currency} {SubscriptionPricingDisplay.Base}</td></tr>
                {collarLine}
                <tr><td style="padding:12px 0 0;border-top:1px solid #eee;font-weight:bold">Total a pagar</td><td style="padding:12px 0 0;border-top:1px solid #eee;text-align:right;font-weight:bold;color:#2e7d32">{money}</td></tr>
              </table>
              <p style="color:#999;font-size:12px">Recordatorio automático. Pago simulado con fines de demostración.</p>
            </div>
            """;

        return (subject, html);
    }
}

/// <summary>Display-only price strings, kept here to avoid leaking formatting into the domain pricing.</summary>
internal static class SubscriptionPricingDisplay
{
    public const string Base   = "149.00";
    public const string Collar = "25.00";
}
