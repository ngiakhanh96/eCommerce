using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace eCommerce.EventBus.Resilience;

/// <summary>
/// Provides resilience policies for event publishing operations.
/// Uses Polly for retry and circuit breaker patterns to handle transient failures.
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Creates a retry policy with exponential backoff.
    /// Retries 3 times with delays of 1s, 2s, and 4s.
    /// </summary>
    /// <param name="logger">Logger for recording retry attempts.</param>
    /// <returns>An async retry policy.</returns>
    public static AsyncRetryPolicy CreateRetryPolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "Retry attempt {RetryCount} after {DelayMs}ms due to: {ErrorMessage}",
                        retryCount,
                        timeSpan.TotalMilliseconds,
                        exception.Message);
                });
    }

    /// <summary>
    /// Creates a circuit breaker policy.
    /// Opens circuit after 5 consecutive failures, stays open for 30 seconds.
    /// </summary>
    /// <param name="logger">Logger for recording circuit state changes.</param>
    /// <returns>An async circuit breaker policy.</returns>
    public static AsyncCircuitBreakerPolicy CreateCircuitBreakerPolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    logger.LogError(
                        exception,
                        "Circuit breaker OPEN for {DurationSeconds}s due to: {ErrorMessage}",
                        duration.TotalSeconds,
                        exception.Message);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker RESET - resuming normal operations");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit breaker HALF-OPEN - testing connection");
                });
    }

    /// <summary>
    /// Creates a combined policy with retry wrapped in circuit breaker.
    /// The circuit breaker protects against cascading failures when retries keep failing.
    /// </summary>
    /// <param name="logger">Logger for recording policy events.</param>
    /// <returns>A combined async policy with circuit breaker and retry.</returns>
    public static IAsyncPolicy CreateResilientPolicy(ILogger logger)
    {
        var retryPolicy = CreateRetryPolicy(logger);
        var circuitBreakerPolicy = CreateCircuitBreakerPolicy(logger);

        // Circuit breaker wraps retry - if retries keep failing, circuit opens
        return Policy.WrapAsync(circuitBreakerPolicy, retryPolicy);
    }
}
