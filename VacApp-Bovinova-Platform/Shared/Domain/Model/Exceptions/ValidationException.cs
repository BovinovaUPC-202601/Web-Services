namespace VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions
{
    /// <summary>The request was malformed or failed validation. Maps to HTTP 400.</summary>
    public class ValidationException(string message) : AppException(message, 400);
}
