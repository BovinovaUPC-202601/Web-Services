namespace VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions
{
    /// <summary>
    /// The upstream model provider throttled the request (too many tokens/requests
    /// per minute). Maps to HTTP 429 so the client can show a "try again" message
    /// instead of a generic 500.
    /// </summary>
    public class RateLimitedException(string message) : AppException(message, 429);
}
