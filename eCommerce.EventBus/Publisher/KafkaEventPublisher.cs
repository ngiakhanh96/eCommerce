using Confluent.Kafka;
using eCommerce.EventBus.IntegrationEvents;
using System.Diagnostics;
using System.Text.Json;

namespace eCommerce.EventBus.Publisher;

/// <summary>
/// Kafka implementation of the event publisher.
/// </summary>
public class KafkaEventPublisher : IEventPublisher
{
    private readonly IProducer<string, string> _producer;

    public KafkaEventPublisher(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public async Task PublishAsync(OutgoingIntegrationEvent integrationEvent)
    {
        try
        {
            var topic = integrationEvent.TopicName;
            var message = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType())
            };

            await _producer.ProduceAsync(topic, message);
        }
        catch (ProduceException<string, string> ex)
        {
            throw new InvalidOperationException($"Failed to publish event: {ex.Message}", ex);
        }
    }
}
