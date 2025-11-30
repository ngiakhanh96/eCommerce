using System.Text.Json;
using eCommerce.UserService.Domain.References;
using eCommerce.UserService.Infrastructure.EventHandlers;
using eCommerce.UserService.Infrastructure.IntegrationEvents.Incoming;
using Microsoft.Extensions.Logging;
using Moq;

namespace eCommerce.UserService.Tests.Infrastructure.EventHandlers;

/// <summary>
/// Unit tests for OrderCreatedEventHandler.
/// </summary>
public class OrderCreatedEventHandlerTests
{
    private readonly Mock<IRefOrderRepository> _mockRefOrderRepository;
    private readonly Mock<ILogger<OrderCreatedEventHandler>> _mockLogger;
    private readonly OrderCreatedEventHandler _handler;

    public OrderCreatedEventHandlerTests()
    {
        _mockRefOrderRepository = new Mock<IRefOrderRepository>();
        _mockLogger = new Mock<ILogger<OrderCreatedEventHandler>>();
        _handler = new OrderCreatedEventHandler(_mockLogger.Object, _mockRefOrderRepository.Object);
    }

    private static string SerializeEvent(OrderCreatedIntegrationEvent eventDto)
    {
        return JsonSerializer.Serialize(eventDto);
    }

    [Fact]
    public async Task HandleAsync_WithValidEvent_ShouldCreateRefOrder()
    {
        // Arrange
        var eventDto = new OrderCreatedIntegrationEvent
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Product = "Laptop",
            Quantity = 2,
            Price = 999.99m
        };

        _mockRefOrderRepository
            .Setup(x => x.GetByIdAsync(eventDto.Id))
            .ReturnsAsync((RefOrder?)null);

        _mockRefOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<RefOrder>()))
            .Returns(Task.CompletedTask);

        _mockRefOrderRepository
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(SerializeEvent(eventDto));

        // Assert
        _mockRefOrderRepository.Verify(
            x => x.AddAsync(It.Is<RefOrder>(o =>
                o.Id == eventDto.Id &&
                o.UserId == eventDto.UserId &&
                o.Product == eventDto.Product &&
                o.Quantity == eventDto.Quantity &&
                o.Price == eventDto.Price)),
            Times.Once);

        _mockRefOrderRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullJsonDeserialization_ShouldNotCreateRefOrder()
    {
        // Arrange - "null" JSON deserializes to null object
        var nullJson = "null";

        // Act
        await _handler.HandleAsync(nullJson);

        // Assert
        _mockRefOrderRepository.Verify(
            x => x.AddAsync(It.IsAny<RefOrder>()),
            Times.Never);

        _mockRefOrderRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithExistingOrder_ShouldNotCreateDuplicate()
    {
        // Arrange
        var eventDto = new OrderCreatedIntegrationEvent
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Product = "Laptop",
            Quantity = 2,
            Price = 999.99m
        };

        var existingOrder = new RefOrder
        {
            Id = eventDto.Id,
            UserId = eventDto.UserId,
            Product = eventDto.Product,
            Quantity = eventDto.Quantity,
            Price = eventDto.Price,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRefOrderRepository
            .Setup(x => x.GetByIdAsync(eventDto.Id))
            .ReturnsAsync(existingOrder);

        // Act
        await _handler.HandleAsync(SerializeEvent(eventDto));

        // Assert
        _mockRefOrderRepository.Verify(
            x => x.AddAsync(It.IsAny<RefOrder>()),
            Times.Never);

        _mockRefOrderRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_ShouldRethrowException()
    {
        // Arrange
        var eventDto = new OrderCreatedIntegrationEvent
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Product = "Laptop",
            Quantity = 2,
            Price = 999.99m
        };

        _mockRefOrderRepository
            .Setup(x => x.GetByIdAsync(eventDto.Id))
            .ReturnsAsync((RefOrder?)null);

        _mockRefOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<RefOrder>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(SerializeEvent(eventDto)));
    }

    [Fact]
    public async Task HandleAsync_WithValidEvent_ShouldSetTimestamps()
    {
        // Arrange
        var eventDto = new OrderCreatedIntegrationEvent
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Product = "Laptop",
            Quantity = 2,
            Price = 999.99m
        };

        RefOrder? capturedOrder = null;

        _mockRefOrderRepository
            .Setup(x => x.GetByIdAsync(eventDto.Id))
            .ReturnsAsync((RefOrder?)null);

        _mockRefOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<RefOrder>()))
            .Callback<RefOrder>(o => capturedOrder = o)
            .Returns(Task.CompletedTask);

        _mockRefOrderRepository
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        var beforeTest = DateTime.UtcNow;

        // Act
        await _handler.HandleAsync(SerializeEvent(eventDto));

        var afterTest = DateTime.UtcNow;

        // Assert
        Assert.NotNull(capturedOrder);
        Assert.True(capturedOrder.CreatedAt >= beforeTest && capturedOrder.CreatedAt <= afterTest);
        Assert.True(capturedOrder.UpdatedAt >= beforeTest && capturedOrder.UpdatedAt <= afterTest);
    }

    [Fact]
    public async Task HandleAsync_WithNullJsonDeserialization_ShouldLogWarning()
    {
        // Arrange - "null" JSON deserializes to null object
        var nullJson = "null";

        // Act
        await _handler.HandleAsync(nullJson);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to deserialize")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithExistingOrder_ShouldLogInformation()
    {
        // Arrange
        var eventDto = new OrderCreatedIntegrationEvent
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Product = "Laptop",
            Quantity = 2,
            Price = 999.99m
        };

        var existingOrder = new RefOrder
        {
            Id = eventDto.Id,
            UserId = eventDto.UserId,
            Product = eventDto.Product,
            Quantity = eventDto.Quantity,
            Price = eventDto.Price,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRefOrderRepository
            .Setup(x => x.GetByIdAsync(eventDto.Id))
            .ReturnsAsync(existingOrder);

        // Act
        await _handler.HandleAsync(SerializeEvent(eventDto));

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already exists")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_OnSuccess_ShouldLogInformation()
    {
        // Arrange
        var eventDto = new OrderCreatedIntegrationEvent
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Product = "Laptop",
            Quantity = 2,
            Price = 999.99m
        };

        _mockRefOrderRepository
            .Setup(x => x.GetByIdAsync(eventDto.Id))
            .ReturnsAsync((RefOrder?)null);

        _mockRefOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<RefOrder>()))
            .Returns(Task.CompletedTask);

        _mockRefOrderRepository
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(SerializeEvent(eventDto));

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("created successfully")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(1, 100.00)]
    [InlineData(10, 1000.00)]
    [InlineData(100, 9999.99)]
    public async Task HandleAsync_WithVariousQuantitiesAndPrices_ShouldCreateRefOrder(int quantity, decimal price)
    {
        // Arrange
        var eventDto = new OrderCreatedIntegrationEvent
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Product = "Test Product",
            Quantity = quantity,
            Price = price
        };

        _mockRefOrderRepository
            .Setup(x => x.GetByIdAsync(eventDto.Id))
            .ReturnsAsync((RefOrder?)null);

        _mockRefOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<RefOrder>()))
            .Returns(Task.CompletedTask);

        _mockRefOrderRepository
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(SerializeEvent(eventDto));

        // Assert
        _mockRefOrderRepository.Verify(
            x => x.AddAsync(It.Is<RefOrder>(o =>
                o.Quantity == quantity &&
                o.Price == price)),
            Times.Once);
    }
}
