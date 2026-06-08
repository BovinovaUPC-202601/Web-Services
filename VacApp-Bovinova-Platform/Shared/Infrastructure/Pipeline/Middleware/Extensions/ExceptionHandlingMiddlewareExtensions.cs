using VacApp_Bovinova_Platform.Shared.Infrastructure.Pipeline.Middleware.Components;

namespace VacApp_Bovinova_Platform.Shared.Infrastructure.Pipeline.Middleware.Extensions
{
    public static class ExceptionHandlingMiddlewareExtensions
    {
        /// <summary>Registers the global exception-to-HTTP-status handler. Place right after UseCors.</summary>
        public static IApplicationBuilder UseRequestExceptionHandling(this IApplicationBuilder builder)
            => builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
