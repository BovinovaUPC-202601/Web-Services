namespace VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions
{
    /// <summary>The authenticated user lacks permission for this operation (e.g. inactive staff, insufficient access level). Maps to HTTP 403.</summary>
    public class ForbiddenRequestException(string message) : AppException(message, 403);
}
