using eCommerce.EventBus.IntegrationEvents;
using eCommerce.EventBus.Resilience;
using Microsoft.Extensions.Logging;
using Polly;

namespace eCommerce.EventBus.Publisher;

/// <summary>
/// A resilient event publisher decorator that wraps another publisher with retry and circuit breaker policies.
/// Implements the Decorator pattern to add resilience without modifying the underlying publisher.
/// </summary>
public class ResilientEventPublisher : IEventPublisher
{
    private readonly IEventPublisher _innerPublisher;
    private readonly IAsyncPolicy _resiliencePolicy;
    private readonly ILogger<ResilientEventPublisher> _logger;

    /// <summary>
    /// Initializes a new instance of the ResilientEventPublisher.
    /// </summary>
    /// <param name="innerPublisher">The underlying event publisher to wrap.</param>
    /// <param name="logger">Logger for recording events.</param>
    public ResilientEventPublisher(
        IEventPublisher innerPublisher,
        ILogger<ResilientEventPublisher> logger)
    {
        _innerPublisher = innerPublisher ?? throw new ArgumentNullException(nameof(innerPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _resiliencePolicy = ResiliencePolicies.CreateResilientPolicy(logger);
    }

    /// <summary>
    /// Publishes an event with automatic retry and circuit breaker protection.
    /// </summary>
    /// <param name="integrationEvent">The event to publish.</param>
    public async Task PublishAsync(OutgoingIntegrationEvent integrationEvent)
    {
        try
        {
            await _resiliencePolicy.ExecuteAsync(async () =>
            {
                await _innerPublisher.PublishAsync(integrationEvent);
            });
        }
        catch (Exception ex)
        {
            var eventType = integrationEvent.GetType().Name;
            var topic = integrationEvent.TopicName;
            _logger.LogError(
                ex,
                "Failed to publish event {EventType} to topic {Topic} after all retry attempts",
                eventType,
                topic);
            throw;
        }
    }
}
