using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Infrastructure.Email;

/// <summary>
/// <see cref="IEmailSender"/> backed by Resend (https://resend.com). Talks the HTTP API
/// directly (no SDK). Config via env:
///   RESEND_API_KEY   — required to enable this adapter (else the no-op logger is used).
///   RESEND_FROM      — sender; defaults to Resend's test sender onboarding@resend.dev.
///   RESEND_TO_OVERRIDE — when set, every recipient is replaced by this address.
///
/// Test-mode trap: with no verified domain, Resend only sends FROM onboarding@resend.dev
/// and ONLY to the account owner's email. RESEND_TO_OVERRIDE pins delivery to that
/// address so the demo works without DNS verification.
/// </summary>
public class ResendEmailSender(HttpClient http) : IEmailSender
{
    private readonly string _apiKey  = Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? string.Empty;
    private readonly string _from    = Environment.GetEnvironmentVariable("RESEND_FROM") ?? "onboarding@resend.dev";
    private readonly string? _override = Environment.GetEnvironmentVariable("RESEND_TO_OVERRIDE");

    public async Task SendAsync(string to, string subject, string html, string? idempotencyKey = null)
    {
        var recipient = string.IsNullOrWhiteSpace(_override) ? to : _override;

        var payload = JsonSerializer.Serialize(new
        {
            from    = _from,
            to      = new[] { recipient },
            subject,
            html
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
            request.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);

        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
