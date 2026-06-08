namespace VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions
{
    /// <summary>The requested resource does not exist. Maps to HTTP 404.</summary>
    public class NotFoundException(string message) : AppException(message, 404);
}
