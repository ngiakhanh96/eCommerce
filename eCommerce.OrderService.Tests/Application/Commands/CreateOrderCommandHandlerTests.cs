using eCommerce.EventBus.Publisher;
using eCommerce.OrderService.Application.Commands;
using eCommerce.OrderService.Application.Dtos;
using eCommerce.OrderService.Domain.AggregatesModel.OrderAggregate;
using eCommerce.OrderService.Domain.References;
using eCommerce.OrderService.Infrastructure.IntegrationEvents.Outgoing;

namespace eCommerce.OrderService.Tests.Application.Commands;

/// <summary>
/// Unit tests for CreateOrderCommandHandler.
/// Tests focus on command handling logic with mocked dependencies.
/// </summary>
public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IRefUserRepository> _mockRefUserRepository;
    private readonly Mock<IEventPublisher> _mockEventPublisher;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockRefUserRepository = new Mock<IRefUserRepository>();
        _mockEventPublisher = new Mock<IEventPublisher>();

        _handler = new CreateOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockRefUserRepository.Object,
            _mockEventPublisher.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateOrderAndPublishEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            UserId = userId,
            Product = "Laptop",
            Quantity = 2,
            Price = 999.99m
        };

        var refUser = new RefUser
        {
            Id = userId,
            Name = "John Doe",
            Email = "john@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _mockRefUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(refUser);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        _mockOrderRepository
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockEventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<OrderCreatedIntegrationEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<OrderDto>(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Laptop", result.Product);
        Assert.Equal(2, result.Quantity);
        Assert.Equal(999.99m, result.Price);
        Assert.NotEqual(Guid.Empty, result.Id);

        // Verify repository was called
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Once);
        _mockOrderRepository.Verify(x => x.SaveChangesAsync(), Times.Once);

        // Verify event was published
        _mockEventPublisher.Verify(x => x.PublishAsync(
            It.Is<OrderCreatedIntegrationEvent>(e =>
                e.UserId == userId &&
                e.Product == "Laptop" &&
                e.Quantity == 2 &&
                e.Price == 999.99m)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            UserId = userId,
            Product = "Laptop",
            Quantity = 2,
            Price = 999.99m
        };

        _mockRefUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((RefUser?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.HandleAsync(command));

        Assert.Contains(userId.ToString(), exception.Message);
        Assert.Contains("does not exist", exception.Message);

        // Verify order was NOT created
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Never);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<OrderCreatedIntegrationEvent>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryFails_ShouldNotPublishEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            UserId = userId,
            Product = "Laptop",
            Quantity = 2,
            Price = 999.99m
        };

        var refUser = new RefUser
        {
            Id = userId,
            Name = "John Doe",
            Email = "john@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _mockRefUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(refUser);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(command));

        // Verify event was NOT published
        _mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<OrderCreatedIntegrationEvent>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectDataToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            UserId = userId,
            Product = "Keyboard",
            Quantity = 5,
            Price = 149.99m
        };

        var refUser = new RefUser { Id = userId, Name = "Test User", Email = "test@test.com" };
        Order? capturedOrder = null;

        _mockRefUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(refUser);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order => capturedOrder = order)
            .Returns(Task.CompletedTask);

        _mockOrderRepository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);
        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<OrderCreatedIntegrationEvent>())).Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(capturedOrder);
        Assert.Equal(userId, capturedOrder.UserId);
        Assert.Equal("Keyboard", capturedOrder.Product);
        Assert.Equal(5, capturedOrder.Quantity);
        Assert.Equal(149.99m, capturedOrder.Price);
    }
}
