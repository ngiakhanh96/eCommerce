using eCommerce.EventBus.IntegrationEvents;
using eCommerce.EventBus.Publisher;
using Microsoft.Extensions.Logging;

namespace eCommerce.EventBus.Tests.Publisher;

/// <summary>
/// Unit tests for ResilientEventPublisher.
/// </summary>
public class ResilientEventPublisherTests
{
    #region Test Events

    public class TestIntegrationEvent : OutgoingIntegrationEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public TestIntegrationEvent() : base("test-topic") { }
    }

    #endregion

    [Fact]
    public async Task PublishAsync_WithSuccessfulInnerPublisher_ShouldSucceed()
    {
        // Arrange
        var mockInnerPublisher = new Mock<IEventPublisher>();
        var mockLogger = new Mock<ILogger<ResilientEventPublisher>>();

        mockInnerPublisher
            .Setup(x => x.PublishAsync(It.IsAny<OutgoingIntegrationEvent>()))
            .Returns(Task.CompletedTask);

        var publisher = new ResilientEventPublisher(mockInnerPublisher.Object, mockLogger.Object);
        var testEvent = new TestIntegrationEvent { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        await publisher.PublishAsync(testEvent);

        // Assert
        mockInnerPublisher.Verify(x => x.PublishAsync(testEvent), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WhenInnerPublisherFails_ShouldRetryAndEventuallySucceed()
    {
        // Arrange
        var mockInnerPublisher = new Mock<IEventPublisher>();
        var mockLogger = new Mock<ILogger<ResilientEventPublisher>>();
        var callCount = 0;

        mockInnerPublisher
            .Setup(x => x.PublishAsync(It.IsAny<OutgoingIntegrationEvent>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount < 3)
                {
                    throw new Exception("Transient failure");
                }
                return Task.CompletedTask;
            });

        var publisher = new ResilientEventPublisher(mockInnerPublisher.Object, mockLogger.Object);
        var testEvent = new TestIntegrationEvent { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        await publisher.PublishAsync(testEvent);

        // Assert
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task PublishAsync_WhenInnerPublisherFailsAllRetries_ShouldThrowException()
    {
        // Arrange
        var mockInnerPublisher = new Mock<IEventPublisher>();
        var mockLogger = new Mock<ILogger<ResilientEventPublisher>>();

        mockInnerPublisher
            .Setup(x => x.PublishAsync(It.IsAny<OutgoingIntegrationEvent>()))
            .ThrowsAsync(new Exception("Persistent failure"));

        var publisher = new ResilientEventPublisher(mockInnerPublisher.Object, mockLogger.Object);
        var testEvent = new TestIntegrationEvent { Id = Guid.NewGuid(), Name = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => publisher.PublishAsync(testEvent));
    }

    [Fact]
    public void Constructor_WithNullInnerPublisher_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ResilientEventPublisher>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ResilientEventPublisher(null!, mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockInnerPublisher = new Mock<IEventPublisher>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ResilientEventPublisher(mockInnerPublisher.Object, null!));
    }

    [Fact]
    public async Task PublishAsync_ShouldLogDebugBeforePublishing()
    {
        // Arrange
        var mockInnerPublisher = new Mock<IEventPublisher>();
        var mockLogger = new Mock<ILogger<ResilientEventPublisher>>();

        mockInnerPublisher
            .Setup(x => x.PublishAsync(It.IsAny<OutgoingIntegrationEvent>()))
            .Returns(Task.CompletedTask);

        var publisher = new ResilientEventPublisher(mockInnerPublisher.Object, mockLogger.Object);
        var testEvent = new TestIntegrationEvent { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        await publisher.PublishAsync(testEvent);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Publishing event")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_OnSuccess_ShouldLogInformation()
    {
        // Arrange
        var mockInnerPublisher = new Mock<IEventPublisher>();
        var mockLogger = new Mock<ILogger<ResilientEventPublisher>>();

        mockInnerPublisher
            .Setup(x => x.PublishAsync(It.IsAny<OutgoingIntegrationEvent>()))
            .Returns(Task.CompletedTask);

        var publisher = new ResilientEventPublisher(mockInnerPublisher.Object, mockLogger.Object);
        var testEvent = new TestIntegrationEvent { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        await publisher.PublishAsync(testEvent);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully published event")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_OnFailure_ShouldLogError()
    {
        // Arrange
        var mockInnerPublisher = new Mock<IEventPublisher>();
        var mockLogger = new Mock<ILogger<ResilientEventPublisher>>();

        mockInnerPublisher
            .Setup(x => x.PublishAsync(It.IsAny<OutgoingIntegrationEvent>()))
            .ThrowsAsync(new Exception("Test failure"));

        var publisher = new ResilientEventPublisher(mockInnerPublisher.Object, mockLogger.Object);
        var testEvent = new TestIntegrationEvent { Id = Guid.NewGuid(), Name = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => publisher.PublishAsync(testEvent));

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to publish event")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
