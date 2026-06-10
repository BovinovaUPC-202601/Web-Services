using System.Text.Json;
using VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions;

namespace VacApp_Bovinova_Platform.Shared.Infrastructure.Pipeline.Middleware.Components
{
    /// <summary>
    /// Catches exceptions thrown further down the pipeline and turns them into a
    /// proper JSON HTTP response. <see cref="AppException"/>s are mapped to their
    /// declared status code (e.g. 409, 401); anything else becomes a 500 with a
    /// generic message (the real error is logged, not leaked to the client).
    ///
    /// This must be registered AFTER UseCors so the error response still carries
    /// the CORS headers; otherwise the browser reports it as a "Network Error"
    /// instead of letting the frontend read the message.
    /// </summary>
    public class ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (AppException exception)
            {
                await WriteErrorAsync(context, exception.StatusCode, exception.Message);
            }
            catch (Exception exception)
            {
                logger.LogError(exception,
                    "Unhandled exception processing {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                await WriteErrorAsync(context, StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
            }
        }

        private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
        {
            // If the response already began streaming we can't change the status; just bail.
            if (context.Response.HasStarted)
                return;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new { message, status = statusCode });
            await context.Response.WriteAsync(payload);
        }
    }
}
