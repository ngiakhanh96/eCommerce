namespace eCommerce.EventBus.Subscriber;

/// <summary>
/// Interface for event subscribers that consume events from message brokers.
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Starts consuming events from the message broker.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops consuming events from the message broker.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken);
}
