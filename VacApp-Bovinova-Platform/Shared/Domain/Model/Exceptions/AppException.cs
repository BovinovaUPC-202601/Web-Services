namespace VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions
{
    /// <summary>
    /// Base class for expected application/domain errors that should be surfaced
    /// to the client with a specific HTTP status code (instead of a raw 500).
    /// The global <c>ExceptionHandlingMiddleware</c> reads <see cref="StatusCode"/>.
    /// </summary>
    public abstract class AppException(string message, int statusCode) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
    }
}
