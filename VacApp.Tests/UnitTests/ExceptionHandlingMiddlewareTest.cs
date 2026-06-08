using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Pipeline.Middleware.Components;

namespace VacApp.Tests.UnitTests;

/// <summary>
/// Verifies the global exception handler maps exceptions to the right HTTP status
/// codes (the fix for "duplicate user / invalid token -> 500 -> Network Error").
/// Pure middleware test: no host, no database.
/// </summary>
public class ExceptionHandlingMiddlewareTest
{
    private static async Task<(int status, string body)> InvokeWith(RequestDelegate next)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            next, NullLogger<ExceptionHandlingMiddleware>.Instance);
        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return (context.Response.StatusCode, body);
    }

    [Fact]
    public async Task MapsConflictExceptionTo409WithMessage()
    {
        var (status, body) = await InvokeWith(_ => throw new ConflictException("User already exists"));

        Assert.Equal((int)HttpStatusCode.Conflict, status);
        Assert.Contains("User already exists", body);
    }

    [Fact]
    public async Task MapsUnauthorizedRequestExceptionTo401()
    {
        var (status, _) = await InvokeWith(_ => throw new UnauthorizedRequestException("Invalid token"));

        Assert.Equal((int)HttpStatusCode.Unauthorized, status);
    }

    [Fact]
    public async Task MapsUnknownExceptionTo500WithoutLeakingDetails()
    {
        var (status, body) = await InvokeWith(_ => throw new InvalidOperationException("secret internal detail"));

        Assert.Equal((int)HttpStatusCode.InternalServerError, status);
        Assert.DoesNotContain("secret internal detail", body);
    }

    [Fact]
    public async Task PassesThroughWhenNoExceptionIsThrown()
    {
        var nextWasCalled = false;

        var (status, _) = await InvokeWith(ctx =>
        {
            nextWasCalled = true;
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        });

        Assert.True(nextWasCalled);
        Assert.Equal(StatusCodes.Status200OK, status);
    }
}
