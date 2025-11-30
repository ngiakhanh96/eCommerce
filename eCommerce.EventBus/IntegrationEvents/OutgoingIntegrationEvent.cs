namespace eCommerce.EventBus.IntegrationEvents;

public abstract class OutgoingIntegrationEvent
{
    public string TopicName { get; init; }

    protected OutgoingIntegrationEvent(string topicName)
    {
        TopicName = topicName;
    }
}