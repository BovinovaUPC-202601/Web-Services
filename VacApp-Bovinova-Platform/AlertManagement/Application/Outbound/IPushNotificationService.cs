namespace VacApp_Bovinova_Platform.AlertManagement.Application.Outbound;

/// <summary>
/// Outbound port for background push notifications to the rancher's mobile app. The
/// Application layer depends only on this abstraction; the concrete adapter (OneSignal,
/// or a no-op logger fallback) lives in Infrastructure, so the provider can change
/// without touching the domain (hexagonal). A failure here must NEVER break alert
/// creation — callers dispatch on a best-effort basis.
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Delivers a push to every device the user is logged in on. The user is addressed by
    /// their VacApp user id, mapped to the provider's external-user alias on the device
    /// (the mobile app calls OneSignal.login(userId) after sign-in).
    /// </summary>
    Task SendToUserAsync(int userId, string title, string message, CancellationToken cancellationToken = default);
}
