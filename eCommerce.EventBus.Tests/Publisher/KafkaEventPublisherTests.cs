using Confluent.Kafka;
using eCommerce.EventBus.IntegrationEvents;
using eCommerce.EventBus.Publisher;

namespace eCommerce.EventBus.Tests.Publisher;

/// <summary>
/// Unit tests for KafkaEventPublisher.
/// </summary>
public class KafkaEventPublisherTests
{
    #region Test Events

    public class TestIntegrationEvent : OutgoingIntegrationEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public TestIntegrationEvent() : base("test-topic") { }
        public TestIntegrationEvent(string topicName) : base(topicName) { }
    }

    public class AnotherIntegrationEvent : OutgoingIntegrationEvent
    {
        public int Value { get; set; }

        public AnotherIntegrationEvent() : base("another-topic") { }
    }

    #endregion

    [Fact]
    public async Task PublishAsync_ShouldCallProducerWithCorrectTopic()
    {
        // Arrange
        var mockProducer = new Mock<IProducer<string, string>>();
        string? capturedTopic = null;

        mockProducer
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<string, string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Message<string, string>, CancellationToken>((topic, msg, ct) =>
            {
                capturedTopic = topic;
            })
            .ReturnsAsync(new DeliveryResult<string, string>());

        var publisher = new KafkaEventPublisher(mockProducer.Object);
        var testEvent = new TestIntegrationEvent
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };

        // Act
        await publisher.PublishAsync(testEvent);

        // Assert
        Assert.Equal("test-topic", capturedTopic);
    }

    [Fact]
    public async Task PublishAsync_ShouldSerializeEventToJson()
    {
        // Arrange
        var mockProducer = new Mock<IProducer<string, string>>();
        string? capturedValue = null;

        mockProducer
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<string, string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Message<string, string>, CancellationToken>((topic, msg, ct) =>
            {
                capturedValue = msg.Value;
            })
            .ReturnsAsync(new DeliveryResult<string, string>());

        var publisher = new KafkaEventPublisher(mockProducer.Object);
        var eventId = Guid.NewGuid();
        var testEvent = new TestIntegrationEvent
        {
            Id = eventId,
            Name = "SerializationTest"
        };

        // Act
        await publisher.PublishAsync(testEvent);

        // Assert
        Assert.NotNull(capturedValue);
        Assert.Contains(eventId.ToString(), capturedValue);
        Assert.Contains("SerializationTest", capturedValue);
    }

    [Fact]
    public async Task PublishAsync_ShouldUseGuidAsMessageKey()
    {
        // Arrange
        var mockProducer = new Mock<IProducer<string, string>>();
        string? capturedKey = null;

        mockProducer
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<string, string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Message<string, string>, CancellationToken>((topic, msg, ct) =>
            {
                capturedKey = msg.Key;
            })
            .ReturnsAsync(new DeliveryResult<string, string>());

        var publisher = new KafkaEventPublisher(mockProducer.Object);
        var testEvent = new TestIntegrationEvent { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        await publisher.PublishAsync(testEvent);

        // Assert
        Assert.NotNull(capturedKey);
        Assert.True(Guid.TryParse(capturedKey, out _), "Message key should be a valid GUID");
    }

    [Fact]
    public async Task PublishAsync_WithDifferentTopics_ShouldPublishToCorrectTopic()
    {
        // Arrange
        var mockProducer = new Mock<IProducer<string, string>>();
        var capturedTopics = new List<string>();

        mockProducer
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<string, string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Message<string, string>, CancellationToken>((topic, msg, ct) =>
            {
                capturedTopics.Add(topic);
            })
            .ReturnsAsync(new DeliveryResult<string, string>());

        var publisher = new KafkaEventPublisher(mockProducer.Object);

        // Act
        await publisher.PublishAsync(new TestIntegrationEvent { Id = Guid.NewGuid(), Name = "Test" });
        await publisher.PublishAsync(new AnotherIntegrationEvent { Value = 42 });

        // Assert
        Assert.Equal(2, capturedTopics.Count);
        Assert.Contains("test-topic", capturedTopics);
        Assert.Contains("another-topic", capturedTopics);
    }

    [Fact]
    public async Task PublishAsync_WhenProducerThrows_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mockProducer = new Mock<IProducer<string, string>>();
        mockProducer
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ProduceException<string, string>(
                new Error(ErrorCode.BrokerNotAvailable, "Broker not available"),
                new DeliveryResult<string, string>()));

        var publisher = new KafkaEventPublisher(mockProducer.Object);
        var testEvent = new TestIntegrationEvent { Id = Guid.NewGuid(), Name = "Test" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => publisher.PublishAsync(testEvent));

        Assert.Contains("Failed to publish event", exception.Message);
    }
}
