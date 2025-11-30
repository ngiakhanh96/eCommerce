using eCommerce.EventBus.Subscriber;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eCommerce.EventBus.Extensions;

/// <summary>
/// Hosted service that manages the lifecycle of the Kafka event subscriber.
/// </summary>
public class EventSubscriberHostedService : IHostedService
{
    private readonly IEventSubscriber _eventSubscriber;
    private readonly ILogger<EventSubscriberHostedService> _logger;

    public EventSubscriberHostedService(
        IEventSubscriber eventSubscriber, 
        ILogger<EventSubscriberHostedService> logger)
    {
        _eventSubscriber = eventSubscriber;
        _logger = logger;
    }

    /// <summary>
    /// Starts the event subscriber when the application starts.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting event subscriber hosted service...");
        await _eventSubscriber.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Stops the event subscriber when the application stops.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping event subscriber hosted service...");
        await _eventSubscriber.StopAsync(cancellationToken);
    }
}