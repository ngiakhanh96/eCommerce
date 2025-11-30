using eCommerce.EventBus.IntegrationEvents;

namespace eCommerce.EventBus.Publisher;

/// <summary>
/// Interface for publishing domain events to message brokers.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync(OutgoingIntegrationEvent integrationEvent);
}
