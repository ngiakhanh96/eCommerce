using eCommerce.OrderService.Application.Dtos;
using eCommerce.OrderService.Application.Queries;
using eCommerce.OrderService.Domain.AggregatesModel.OrderAggregate;

namespace eCommerce.OrderService.Tests.Application.Queries;

/// <summary>
/// Unit tests for GetOrderQueryHandler.
/// </summary>
public class GetOrderQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly GetOrderQueryHandler _handler;

    public GetOrderQueryHandlerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _handler = new GetOrderQueryHandler(_mockOrderRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingOrder_ShouldReturnOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetOrderQuery(orderId);

        var order = Order.Create(orderId, userId, "Laptop", 1, 999.99m);

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<OrderDto>(result);
        Assert.Equal(orderId, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Laptop", result.Product);
        Assert.Equal(1, result.Quantity);
        Assert.Equal(999.99m, result.Price);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentOrder_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var query = new GetOrderQuery(orderId);

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.HandleAsync(query));
        Assert.Contains(orderId.ToString(), exception.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetOrderQuery(orderId);

        var order = Order.Create(orderId, userId, "Test Product", 1, 10.00m);

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        await _handler.HandleAsync(query);

        // Assert
        _mockOrderRepository.Verify(x => x.GetByIdAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetOrderQuery(orderId);

        var order = Order.Create(orderId, userId, "Gaming Mouse", 3, 79.99m);

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
        Assert.Equal(order.UserId, result.UserId);
        Assert.Equal(order.Product, result.Product);
        Assert.Equal(order.Quantity, result.Quantity);
        Assert.Equal(order.Price, result.Price);
    }
}
