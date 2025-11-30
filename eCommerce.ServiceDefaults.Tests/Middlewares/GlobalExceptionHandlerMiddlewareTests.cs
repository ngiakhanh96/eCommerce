using System.Text.Json;
using eCommerce.Mediator.Validation;
using eCommerce.ServiceDefaults.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eCommerce.ServiceDefaults.Tests.Middlewares;

/// <summary>
/// Unit tests for GlobalExceptionHandlerMiddleware.
/// </summary>
public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _mockLogger;
    private readonly Mock<IHostEnvironment> _mockEnvironment;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        _mockEnvironment = new Mock<IHostEnvironment>();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/test";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private async Task<T?> ReadResponseBody<T>(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WithKeyNotFoundException_ShouldReturn404()
    {
        // Arrange
        RequestDelegate next = _ => throw new KeyNotFoundException("Resource not found");

        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Contains("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_WithArgumentException_ShouldReturn400()
    {
        // Arrange
        RequestDelegate next = _ => throw new ArgumentException("Invalid argument");

        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithArgumentNullException_ShouldReturn400()
    {
        // Arrange
        RequestDelegate next = _ => throw new ArgumentNullException("param");

        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidOperationException_ShouldReturn400()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Invalid operation");

        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithUnhandledException_ShouldReturn500()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Unexpected error");

        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithException_ShouldLogError()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Test error");

        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unhandled exception")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithRequestValidationException_ShouldReturn400()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Name is required." } }
        };
        RequestDelegate next = _ => throw new RequestValidationException(errors);

        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithRequestValidationException_ShouldLogWarning()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Name is required." } }
        };
        RequestDelegate next = _ => throw new RequestValidationException(errors);

        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation failed")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_InDevelopment_ShouldIncludeStackTrace()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Test error");

        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await ReadResponseBody<Dictionary<string, JsonElement>>(context);
        Assert.NotNull(response);
        Assert.True(response.ContainsKey("stackTrace"));
    }

    [Fact]
    public async Task InvokeAsync_InProduction_ShouldNotIncludeStackTrace()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Test error");

        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await ReadResponseBody<Dictionary<string, JsonElement>>(context);
        Assert.NotNull(response);
        Assert.False(response.ContainsKey("stackTrace"));
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetContentTypeToJson()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Test error");

        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Contains("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_ShouldIncludeTraceIdInResponse()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Test error");

        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await ReadResponseBody<Dictionary<string, JsonElement>>(context);
        Assert.NotNull(response);
        Assert.True(response.ContainsKey("traceId"));
    }

    [Fact]
    public async Task InvokeAsync_ShouldIncludeTimestampInResponse()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Test error");

        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await ReadResponseBody<Dictionary<string, JsonElement>>(context);
        Assert.NotNull(response);
        Assert.True(response.ContainsKey("timestamp"));
    }

    [Fact]
    public async Task InvokeAsync_ShouldIncludeInstancePath()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Test error");

        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
        var context = CreateHttpContext();
        context.Request.Path = "/api/orders/123";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await ReadResponseBody<Dictionary<string, JsonElement>>(context);
        Assert.NotNull(response);
        Assert.True(response.ContainsKey("instance"));
        Assert.Equal("/api/orders/123", response["instance"].GetString());
    }
}
