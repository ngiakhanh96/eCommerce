using System.Text.Json;
using eCommerce.EventBus.EventHandler;

namespace eCommerce.EventBus.Tests.EventHandler;

/// <summary>
/// Unit tests for BaseEventHandler.
/// </summary>
public class BaseEventHandlerTests
{
    #region Test Event and Handler

    public class TestEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class TestEventHandler : BaseEventHandler<TestEvent>
    {
        public TestEvent? ReceivedEvent { get; private set; }
        public int HandleCount { get; private set; }

        protected override Task HandleImplAsync(TestEvent? @event)
        {
            ReceivedEvent = @event;
            HandleCount += @event.Value;
            return Task.CompletedTask;
        }
    }

    public class ThrowingEventHandler : BaseEventHandler<TestEvent>
    {
        protected override Task HandleImplAsync(TestEvent? @event)
        {
            throw new InvalidOperationException("Handler failed");
        }
    }

    public class NullableEventHandler : BaseEventHandler<TestEvent>
    {
        public bool? WasCalledWithNull { get; private set; }

        protected override Task HandleImplAsync(TestEvent? @event)
        {
            WasCalledWithNull = @event == null;
            return Task.CompletedTask;
        }
    }

    #endregion

    [Fact]
    public async Task HandleAsync_ShouldDeserializeJsonAndCallHandler()
    {
        // Arrange
        var handler = new TestEventHandler();
        var testEvent = new TestEvent
        {
            Id = Guid.NewGuid(),
            Name = "TestName",
            Value = 42
        };
        var json = JsonSerializer.Serialize(testEvent);

        // Act
        await handler.HandleAsync(json);

        // Assert
        Assert.NotNull(handler.ReceivedEvent);
        Assert.Equal(testEvent.Id, handler.ReceivedEvent.Id);
        Assert.Equal(testEvent.Name, handler.ReceivedEvent.Name);
        Assert.Equal(testEvent.Value, handler.ReceivedEvent.Value);
    }

    [Fact]
    public async Task HandleAsync_ShouldIncrementHandleCount()
    {
        // Arrange
        var handler = new TestEventHandler();
        var json = JsonSerializer.Serialize(new TestEvent { Id = Guid.NewGuid(), Name = "Test", Value = 1 });

        // Act
        await handler.HandleAsync(json);
        await handler.HandleAsync(json);
        await handler.HandleAsync(json);

        // Assert
        Assert.Equal(3, handler.HandleCount);
    }

    [Fact]
    public async Task HandleAsync_WithNullJson_ShouldPassNullToHandler()
    {
        // Arrange
        var handler = new NullableEventHandler();
        var json = "null";

        // Act
        await handler.HandleAsync(json);

        // Assert
        Assert.True(handler.WasCalledWithNull);
    }

    [Fact]
    public async Task HandleAsync_WhenHandlerThrows_ShouldPropagateException()
    {
        // Arrange
        var handler = new ThrowingEventHandler();
        var json = JsonSerializer.Serialize(new TestEvent { Id = Guid.NewGuid(), Name = "Test", Value = 1 });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(json));

        Assert.Equal("Handler failed", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_WithPartialJson_ShouldDeserializeAvailableFields()
    {
        // Arrange
        var handler = new TestEventHandler();
        var json = "{\"Name\":\"PartialTest\"}";

        // Act
        await handler.HandleAsync(json);

        // Assert
        Assert.NotNull(handler.ReceivedEvent);
        Assert.Equal("PartialTest", handler.ReceivedEvent.Name);
        Assert.Equal(Guid.Empty, handler.ReceivedEvent.Id);
        Assert.Equal(0, handler.ReceivedEvent.Value);
    }

    [Fact]
    public async Task HandleAsync_WithComplexEvent_ShouldDeserializeCorrectly()
    {
        // Arrange
        var handler = new TestEventHandler();
        var eventId = Guid.NewGuid();
        var testEvent = new TestEvent
        {
            Id = eventId,
            Name = "Complex Event with special characters: @#$%",
            Value = int.MaxValue
        };
        var json = JsonSerializer.Serialize(testEvent);

        // Act
        await handler.HandleAsync(json);

        // Assert
        Assert.NotNull(handler.ReceivedEvent);
        Assert.Equal(eventId, handler.ReceivedEvent.Id);
        Assert.Equal("Complex Event with special characters: @#$%", handler.ReceivedEvent.Name);
        Assert.Equal(int.MaxValue, handler.ReceivedEvent.Value);
    }
}
