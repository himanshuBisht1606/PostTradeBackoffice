using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PostTrade.API.Middleware;

namespace PostTrade.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task Invoke_WhenNoException_ShouldPassThrough()
    {
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var logger     = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();
        var middleware = new ExceptionHandlingMiddleware(next, logger);
        var context    = CreateContext();

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Invoke_WhenValidationExceptionThrown_ShouldReturn400()
    {
        var failures = new List<ValidationFailure> { new("Field", "Required") };
        RequestDelegate next = _ => throw new ValidationException(failures);
        var logger     = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();
        var middleware = new ExceptionHandlingMiddleware(next, logger);
        var context    = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Invoke_WhenUnauthorizedAccessExceptionThrown_ShouldReturn401()
    {
        RequestDelegate next = _ => throw new UnauthorizedAccessException("Access denied");
        var logger     = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();
        var middleware = new ExceptionHandlingMiddleware(next, logger);
        var context    = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Invoke_WhenUnhandledExceptionThrown_ShouldReturn500()
    {
        RequestDelegate next = _ => throw new Exception("Unexpected error");
        var logger     = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();
        var middleware = new ExceptionHandlingMiddleware(next, logger);
        var context    = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
    }
}
