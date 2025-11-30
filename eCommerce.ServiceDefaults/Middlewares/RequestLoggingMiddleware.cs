using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace eCommerce.ServiceDefaults.Middlewares;

/// <summary>
/// Middleware that logs structured information about HTTP requests and responses.
/// Captures timing, status codes, and relevant request metadata.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for health check endpoints
        if (IsHealthCheckEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestId = Activity.Current?.Id ?? context.TraceIdentifier;

        try
        {
            _logger.LogInformation(
                "Request started: {Method} {Path} | RequestId: {RequestId} | ContentType: {ContentType}",
                context.Request.Method,
                context.Request.Path,
                requestId,
                context.Request.ContentType ?? "none");

            await _next(context);

            stopwatch.Stop();

            _logger.LogInformation(
                "Request completed: {Method} {Path} | StatusCode: {StatusCode} | Duration: {DurationMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Request failed: {Method} {Path} | Duration: {DurationMs}ms | Error: {ErrorMessage}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw;
        }
    }

    private static bool IsHealthCheckEndpoint(PathString path)
    {
        return path.StartsWithSegments("/health") || path.StartsWithSegments("/alive");
    }
}
