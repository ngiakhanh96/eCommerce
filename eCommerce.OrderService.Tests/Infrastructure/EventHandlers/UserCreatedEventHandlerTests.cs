using System.Text.Json;
using eCommerce.OrderService.Domain.References;
using eCommerce.OrderService.Infrastructure.EventHandlers;
using eCommerce.OrderService.Infrastructure.IntegrationEvents.Incoming;
using Microsoft.Extensions.Logging;

namespace eCommerce.OrderService.Tests.Infrastructure.EventHandlers;

/// <summary>
/// Unit tests for UserCreatedEventHandler.
/// </summary>
public class UserCreatedEventHandlerTests
{
    private readonly Mock<IRefUserRepository> _mockRefUserRepository;
    private readonly Mock<ILogger<UserCreatedEventHandler>> _mockLogger;
    private readonly UserCreatedEventHandler _handler;

    public UserCreatedEventHandlerTests()
    {
        _mockRefUserRepository = new Mock<IRefUserRepository>();
        _mockLogger = new Mock<ILogger<UserCreatedEventHandler>>();
        _handler = new UserCreatedEventHandler(_mockLogger.Object, _mockRefUserRepository.Object);
    }

    private static string SerializeEvent(UserCreatedIntegrationEvent eventDto)
    {
        return JsonSerializer.Serialize(eventDto);
    }

    [Fact]
    public async Task HandleAsync_WithValidEvent_ShouldCreateRefUser()
    {
        // Arrange
        var eventDto = new UserCreatedIntegrationEvent
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john@example.com"
        };

        _mockRefUserRepository
            .Setup(x => x.GetByIdAsync(eventDto.Id))
            .ReturnsAsync((RefUser?)null);

        _mockRefUserRepository
            .Setup(x => x.AddAsync(It.IsAny<RefUser>()))
            .Returns(Task.CompletedTask);

        _mockRefUserRepository
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(SerializeEvent(eventDto));

        // Assert
        _mockRefUserRepository.Verify(
            x => x.AddAsync(It.Is<RefUser>(u =>
                u.Id == eventDto.Id &&
                u.Name == eventDto.Name &&
                u.Email == eventDto.Email)),
            Times.Once);

        _mockRefUserRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullJsonDeserialization_ShouldNotCreateRefUser()
    {
        // Arrange - "null" JSON deserializes to null object
        var nullJson = "null";

        // Act
        await _handler.HandleAsync(nullJson);

        // Assert
        _mockRefUserRepository.Verify(
            x => x.AddAsync(It.IsAny<RefUser>()),
            Times.Never);

        _mockRefUserRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithExistingUser_ShouldNotCreateDuplicate()
    {
        // Arrange
        var eventDto = new UserCreatedIntegrationEvent
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john@example.com"
        };

        var existingUser = new RefUser
        {
            Id = eventDto.Id,
            Name = eventDto.Name,
            Email = eventDto.Email,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRefUserRepository
            .Setup(x => x.GetByIdAsync(eventDto.Id))
            .ReturnsAsync(existingUser);

        // Act
        await _handler.HandleAsync(SerializeEvent(eventDto));

        // Assert
        _mockRefUserRepository.Verify(
            x => x.AddAsync(It.IsAny<RefUser>()),
            Times.Never);

        _mockRefUserRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_ShouldRethrowException()
    {
        // Arrange
        var eventDto = new UserCreatedIntegrationEvent
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john@example.com"
        };

        _mockRefUserRepository
            .Setup(x => x.GetByIdAsync(eventDto.Id))
            .ReturnsAsync((RefUser?)null);

        _mockRefUserRepository
            .Setup(x => x.AddAsync(It.IsAny<RefUser>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(SerializeEvent(eventDto)));
    }

    [Fact]
    public async Task HandleAsync_WithValidEvent_ShouldSetCreatedAt()
    {
        // Arrange
        var eventDto = new UserCreatedIntegrationEvent
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john@example.com"
        };

        RefUser? capturedUser = null;

        _mockRefUserRepository
            .Setup(x => x.GetByIdAsync(eventDto.Id))
            .ReturnsAsync((RefUser?)null);

        _mockRefUserRepository
            .Setup(x => x.AddAsync(It.IsAny<RefUser>()))
            .Callback<RefUser>(u => capturedUser = u)
            .Returns(Task.CompletedTask);

        _mockRefUserRepository
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        var beforeTest = DateTime.UtcNow;

        // Act
        await _handler.HandleAsync(SerializeEvent(eventDto));

        var afterTest = DateTime.UtcNow;

        // Assert
        Assert.NotNull(capturedUser);
        Assert.True(capturedUser.CreatedAt >= beforeTest && capturedUser.CreatedAt <= afterTest);
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
    public async Task HandleAsync_WithExistingUser_ShouldLogInformation()
    {
        // Arrange
        var eventDto = new UserCreatedIntegrationEvent
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john@example.com"
        };

        var existingUser = new RefUser
        {
            Id = eventDto.Id,
            Name = eventDto.Name,
            Email = eventDto.Email,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRefUserRepository
            .Setup(x => x.GetByIdAsync(eventDto.Id))
            .ReturnsAsync(existingUser);

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
        var eventDto = new UserCreatedIntegrationEvent
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john@example.com"
        };

        _mockRefUserRepository
            .Setup(x => x.GetByIdAsync(eventDto.Id))
            .ReturnsAsync((RefUser?)null);

        _mockRefUserRepository
            .Setup(x => x.AddAsync(It.IsAny<RefUser>()))
            .Returns(Task.CompletedTask);

        _mockRefUserRepository
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
}
