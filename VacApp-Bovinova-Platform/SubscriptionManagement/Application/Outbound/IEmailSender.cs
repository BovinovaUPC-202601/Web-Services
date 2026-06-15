namespace VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;

/// <summary>
/// Outbound port for transactional email (e.g. the purchase receipt). The Application
/// layer depends only on this abstraction; the concrete adapter (Resend, or a no-op
/// logger fallback) lives in Infrastructure, so the provider can change without
/// touching the domain (hexagonal). A failure here must never break the payment.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an HTML email. <paramref name="idempotencyKey"/> lets the provider drop
    /// duplicate sends on retries (same checkout → same key → one email).
    /// </summary>
    Task SendAsync(string to, string subject, string html, string? idempotencyKey = null);
}
