namespace eCommerce.EventBus.IntegrationEvents;

public abstract class OutgoingIntegrationEvent
{
    public string TopicName { get; init; }
    public string ParentId { get; init; }

    protected OutgoingIntegrationEvent(string topicName)
    {
        TopicName = topicName;
    }
}