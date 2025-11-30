using eCommerce.UserService.Application.Dtos;
using eCommerce.UserService.Application.Queries;
using eCommerce.UserService.Domain.AggregatesModel.UserAggregate;

namespace eCommerce.UserService.Tests.Application.Queries;

/// <summary>
/// Unit tests for GetUserQueryHandler.
/// </summary>
public class GetUserQueryHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly GetUserQueryHandler _handler;

    public GetUserQueryHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new GetUserQueryHandler(_mockUserRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingUser_ShouldReturnUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserQuery(userId);

        var user = User.Create(userId, "John Doe", "john@example.com");

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<UserDto>(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("john@example.com", result.Email);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserQuery(userId);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.HandleAsync(query));
        Assert.Contains(userId.ToString(), exception.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserQuery(userId);

        var user = User.Create(userId, "Test User", "test@example.com");

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        await _handler.HandleAsync(query);

        // Assert
        _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserQuery(userId);

        var user = User.Create(userId, "Jane Smith", "jane@example.com");

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email, result.Email);
    }
}
