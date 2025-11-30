using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eCommerce.ServiceDefaults.Middlewares;

/// <summary>
/// Global exception handler middleware that catches all unhandled exceptions
/// and returns responses in RFC 7807 Problem Details format.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        // Check if this is a validation exception
        if (IsValidationException(exception))
        {
            _logger.LogWarning(
                "Validation failed. TraceId: {TraceId}, Path: {Path}, Errors: {Errors}",
                traceId,
                context.Request.Path,
                exception.Message);

            var validationProblemDetails = CreateValidationProblemDetails(context, exception, traceId);
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = validationProblemDetails.Status ?? StatusCodes.Status400BadRequest;

            // Must serialize as ValidationProblemDetails to include Errors property
            await context.Response.WriteAsJsonAsync(validationProblemDetails);
        }
        else
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}",
                traceId,
                context.Request.Path);

            var problemDetails = CreateProblemDetails(context, exception, traceId);
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception, string traceId)
    {
        var (statusCode, title, type) = MapExceptionToStatusCode(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Instance = context.Request.Path,
            Detail = GetDetailMessage(exception),
            Extensions =
            {
                // Add trace ID for correlation
                ["traceId"] = traceId,
                // Add timestamp
                ["timestamp"] = DateTime.UtcNow.ToString("o")
            }
        };

        // Include stack trace only in development
        if (_environment.IsDevelopment() && exception.StackTrace != null)
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        // Include inner exception details in development
        if (_environment.IsDevelopment() && exception.InnerException != null)
        {
            problemDetails.Extensions["innerException"] = new
            {
                message = exception.InnerException.Message,
                type = exception.InnerException.GetType().Name
            };
        }

        return problemDetails;
    }

    /// <summary>
    /// Creates a ValidationProblemDetails response for validation exceptions.
    /// </summary>
    private ValidationProblemDetails CreateValidationProblemDetails(HttpContext context, Exception exception, string traceId)
    {
        var errors = ExtractValidationErrors(exception);

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Instance = context.Request.Path,
            Detail = "One or more validation errors occurred.",
            Extensions =
            {
                ["traceId"] = traceId,
                ["timestamp"] = DateTime.UtcNow.ToString("o")
            }
        };

        return problemDetails;
    }

    /// <summary>
    /// Checks if the exception is a validation exception (from Mediator or FluentValidation).
    /// </summary>
    private static bool IsValidationException(Exception exception)
    {
        var typeName = exception.GetType().Name;
        return typeName == "RequestValidationException";
    }

    /// <summary>
    /// Extracts validation errors from a validation exception using reflection.
    /// </summary>
    private static Dictionary<string, string[]> ExtractValidationErrors(Exception exception)
    {
        var errors = new Dictionary<string, string[]>();

        // Try to get Errors property (custom RequestValidationException from Mediator)
        var errorsProperty = exception.GetType().GetProperty("Errors");
        if (errorsProperty != null)
        {
            var errorsValue = errorsProperty.GetValue(exception);
            if (errorsValue is IReadOnlyDictionary<string, string[]> dictErrors)
            {
                foreach (var kvp in dictErrors)
                {
                    errors[kvp.Key] = kvp.Value;
                }
                return errors;
            }
        }
        return errors;
    }

    private static (int StatusCode, string Title, string Type) MapExceptionToStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => (
                StatusCodes.Status400BadRequest,
                "Invalid Request",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            ),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Invalid Argument",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            ),
            InvalidOperationException => (
                StatusCodes.Status400BadRequest,
                "Invalid Operation",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            ),
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            )
        };
    }

    private string GetDetailMessage(Exception exception)
    {
        // In production, don't expose internal error details for 500 errors
        if (!_environment.IsDevelopment() && exception is not (
            ArgumentException or
            ArgumentNullException or
            InvalidOperationException or
            KeyNotFoundException or
            UnauthorizedAccessException))
        {
            return "An unexpected error occurred. Please try again later.";
        }

        return exception.Message;
    }
}
