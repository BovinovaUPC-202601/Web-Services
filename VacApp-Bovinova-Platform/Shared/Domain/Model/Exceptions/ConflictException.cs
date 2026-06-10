namespace VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions
{
    /// <summary>The request conflicts with the current state (e.g. a duplicate resource). Maps to HTTP 409.</summary>
    public class ConflictException(string message) : AppException(message, 409);
}
