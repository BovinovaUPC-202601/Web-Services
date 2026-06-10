namespace VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions
{
    /// <summary>Authentication failed or is missing (bad credentials, missing/invalid token). Maps to HTTP 401.</summary>
    public class UnauthorizedRequestException(string message) : AppException(message, 401);
}
