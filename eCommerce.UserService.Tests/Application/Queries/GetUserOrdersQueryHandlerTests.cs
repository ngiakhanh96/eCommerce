using eCommerce.UserService.Application.Dtos;
using eCommerce.UserService.Application.Queries;
using eCommerce.UserService.Domain.References;

namespace eCommerce.UserService.Tests.Application.Queries;

/// <summary>
/// Unit tests for GetUserOrdersQueryHandler.
/// </summary>
public class GetUserOrdersQueryHandlerTests
{
    private readonly Mock<IRefOrderRepository> _mockRefOrderRepository;
    private readonly GetUserOrdersQueryHandler _handler;

    public GetUserOrdersQueryHandlerTests()
    {
        _mockRefOrderRepository = new Mock<IRefOrderRepository>();
        _handler = new GetUserOrdersQueryHandler(_mockRefOrderRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingOrders_ShouldReturnOrderDtoList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserOrdersQuery(userId);

        var refOrders = new List<RefOrder>
        {
            new RefOrder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Product = "Laptop",
                Quantity = 1,
                Price = 999.99m,
                CreatedAt = DateTime.UtcNow
            },
            new RefOrder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Product = "Mouse",
                Quantity = 2,
                Price = 49.99m,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockRefOrderRepository
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(refOrders);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, dto => Assert.IsType<OrderDto>(dto));
    }

    [Fact]
    public async Task HandleAsync_WithNoOrders_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserOrdersQuery(userId);

        _mockRefOrderRepository
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<RefOrder>());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepositoryWithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserOrdersQuery(userId);

        _mockRefOrderRepository
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<RefOrder>());

        // Act
        await _handler.HandleAsync(query);

        // Assert
        _mockRefOrderRepository.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var query = new GetUserOrdersQuery(userId);

        var refOrder = new RefOrder
        {
            Id = orderId,
            UserId = userId,
            Product = "Keyboard",
            Quantity = 3,
            Price = 149.99m,
            CreatedAt = DateTime.UtcNow
        };

        _mockRefOrderRepository
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<RefOrder> { refOrder });

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Single(result);
        var dto = result.First();
        Assert.Equal(orderId, dto.Id);
        Assert.Equal(userId, dto.UserId);
        Assert.Equal("Keyboard", dto.Product);
        Assert.Equal(3, dto.Quantity);
        Assert.Equal(149.99m, dto.Price);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleOrders_ShouldPreserveAllOrderData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserOrdersQuery(userId);

        var refOrders = Enumerable.Range(1, 5).Select(i => new RefOrder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Product = $"Product {i}",
            Quantity = i,
            Price = i * 10.99m,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _mockRefOrderRepository
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(refOrders);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(5, result.Count);
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(refOrders[i].Id, result[i].Id);
            Assert.Equal(refOrders[i].Product, result[i].Product);
            Assert.Equal(refOrders[i].Quantity, result[i].Quantity);
            Assert.Equal(refOrders[i].Price, result[i].Price);
        }
    }
}
