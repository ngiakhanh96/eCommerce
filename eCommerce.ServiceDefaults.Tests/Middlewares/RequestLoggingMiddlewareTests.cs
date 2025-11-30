using eCommerce.ServiceDefaults.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace eCommerce.ServiceDefaults.Tests.Middlewares;

/// <summary>
/// Unit tests for RequestLoggingMiddleware.
/// </summary>
public class RequestLoggingMiddlewareTests
{
    private readonly Mock<ILogger<RequestLoggingMiddleware>> _mockLogger;

    public RequestLoggingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<RequestLoggingMiddleware>>();
    }

    private static DefaultHttpContext CreateHttpContext(string path = "/test")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNext()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new RequestLoggingMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogRequestStarted()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        var middleware = new RequestLoggingMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/users";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request started")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogRequestCompleted()
    {
        // Arrange
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        };

        var middleware = new RequestLoggingMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request completed")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithException_ShouldLogError()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Test error");

        var middleware = new RequestLoggingMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => middleware.InvokeAsync(context));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request failed")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithException_ShouldRethrow()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test error");
        RequestDelegate next = _ => throw expectedException;

        var middleware = new RequestLoggingMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context));
        Assert.Same(expectedException, thrownException);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    [InlineData("/alive")]
    [InlineData("/alive/liveness")]
    public async Task InvokeAsync_WithHealthCheckEndpoint_ShouldSkipLogging(string path)
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        var middleware = new RequestLoggingMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext(path);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Should not log for health check endpoints
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Theory]
    [InlineData("/api/users")]
    [InlineData("/api/orders")]
    [InlineData("/healthcheck")] // Note: Different from /health
    [InlineData("/liveness")] // Note: Different from /alive
    public async Task InvokeAsync_WithNonHealthCheckEndpoint_ShouldLog(string path)
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        var middleware = new RequestLoggingMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext(path);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Should log for non-health check endpoints
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request started")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogMethodAndPath()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        var middleware = new RequestLoggingMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();
        context.Request.Method = "DELETE";
        context.Request.Path = "/api/orders/42";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("DELETE") && 
                    v.ToString()!.Contains("/api/orders/42")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogStatusCode()
    {
        // Arrange
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 201;
            return Task.CompletedTask;
        };

        var middleware = new RequestLoggingMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("201")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
