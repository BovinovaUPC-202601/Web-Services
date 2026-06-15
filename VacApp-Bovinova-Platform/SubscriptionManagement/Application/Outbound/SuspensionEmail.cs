using System.Globalization;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;

/// <summary>
/// Builds the "subscription suspended for non-payment" email (subject + HTML). Pure
/// formatting. Shows the total that was due (Plus base + extra collars) and invites the
/// user to pay again to reactivate (which restores their collars).
/// </summary>
public static class SuspensionEmail
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
        decimal total, string currency, DateTime renewalUtc, int extraCollars)
    {
        var renewalPeru = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(renewalUtc, DateTimeKind.Utc), PeruTimeZone);
        var renewalDate = renewalPeru.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var money = $"{currency} {total.ToString("0.00", CultureInfo.InvariantCulture)}";

        var collarNote = extraCollars > 0
            ? $" (incluye {extraCollars} collar{(extraCollars == 1 ? "" : "es")} adicional{(extraCollars == 1 ? "" : "es")})"
            : string.Empty;

        var subject = "VacApp — Tu suscripción fue suspendida por falta de pago";

        var html = $"""
            <div style="font-family:Arial,Helvetica,sans-serif;max-width:520px;margin:0 auto;color:#1a1a1a">
              <h2 style="color:#c62828;margin-bottom:4px">VacApp · Bovinova</h2>
              <p style="margin-top:0">Tu plan <strong>Plus</strong> venció el <strong>{renewalDate}</strong> y no se registró el pago, por lo que la suscripción fue <strong>suspendida</strong>.</p>
              <p>Mientras esté suspendida: se desactivan los <strong>collares IoT</strong> y el acceso a IA. Tu cuenta queda en plan <strong>Free</strong>.</p>
              <table style="width:100%;border-collapse:collapse;margin:16px 0">
                <tr><td style="padding:8px 0;color:#777">Monto pendiente del mes{collarNote}</td><td style="padding:8px 0;text-align:right;font-weight:bold;color:#c62828">{money}</td></tr>
              </table>
              <p>Para <strong>reactivar</strong> tu plan y tus collares, ingresá a VacApp y volvé a pagar el plan Plus.</p>
              <p style="color:#999;font-size:12px">Aviso automático. Pago simulado con fines de demostración.</p>
            </div>
            """;

        return (subject, html);
    }
}
