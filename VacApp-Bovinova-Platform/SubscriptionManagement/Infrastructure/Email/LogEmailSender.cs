using Microsoft.Extensions.Logging;
using VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Infrastructure.Email;

/// <summary>
/// No-op <see cref="IEmailSender"/>: logs the email instead of sending it. Used as the
/// default when RESEND_API_KEY is absent, so the demo keeps working with zero external
/// config (the receipt still shows up in the backend log).
/// </summary>
public class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string html, string? idempotencyKey = null)
    {
        // Log the body too: with no Resend configured this is the only place the content
        // (e.g. a password-recovery code) surfaces, so the demo/dev flow stays testable.
        logger.LogInformation(
            "[EMAIL:noop] would send to {To} | subject: {Subject} | idempotency: {Key}\n{Html}",
            to, subject, idempotencyKey ?? "-", html);
        return Task.CompletedTask;
    }
}
