using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VacApp_Bovinova_Platform.AlertManagement.Application.Outbound;

namespace VacApp_Bovinova_Platform.AlertManagement.Infrastructure.Push;

/// <summary>
/// <see cref="IPushNotificationService"/> backed by OneSignal (https://onesignal.com).
/// Talks the REST API directly (no SDK). OneSignal wraps FCM/APNs, so the backend never
/// stores device tokens: it targets the user by external-id alias and OneSignal fans out
/// to every registered device. Config via env:
///   ONESIGNAL_APP_ID       — required; the OneSignal app id.
///   ONESIGNAL_REST_API_KEY — required to enable this adapter (else the no-op logger is used).
///
/// The mobile app maps VacApp userId → OneSignal external_id via OneSignal.login(userId);
/// this adapter must address the same alias for delivery to land.
/// </summary>
public class OneSignalPushSender(HttpClient http) : IPushNotificationService
{
    private const string Endpoint = "https://api.onesignal.com/notifications";

    private readonly string _appId  = Environment.GetEnvironmentVariable("ONESIGNAL_APP_ID") ?? string.Empty;
    private readonly string _apiKey = Environment.GetEnvironmentVariable("ONESIGNAL_REST_API_KEY") ?? string.Empty;

    public async Task SendToUserAsync(int userId, string title, string message, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            app_id         = _appId,
            target_channel = "push",
            include_aliases = new { external_id = new[] { userId.ToString() } },
            headings       = new { en = title,   es = title },
            contents       = new { en = message, es = message }
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        // OneSignal v2 REST auth scheme: "Key <REST_API_KEY>" (not Basic).
        request.Headers.Authorization = new AuthenticationHeaderValue("Key", _apiKey);

        var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
