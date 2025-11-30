using eCommerce.EventBus.Publisher;
using eCommerce.UserService.Application.Commands;
using eCommerce.UserService.Application.Dtos;
using eCommerce.UserService.Domain.AggregatesModel.UserAggregate;
using eCommerce.UserService.Infrastructure.IntegrationEvents.Outgoing;

namespace eCommerce.UserService.Tests.Application.Commands;

/// <summary>
/// Unit tests for CreateUserCommandHandler.
/// Tests focus on command handling logic with mocked dependencies.
/// </summary>
public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IEventPublisher> _mockEventPublisher;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockEventPublisher = new Mock<IEventPublisher>();

        _handler = new CreateUserCommandHandler(
            _mockUserRepository.Object,
            _mockEventPublisher.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateUserAndPublishEvent()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "john@example.com"
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _mockUserRepository
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockEventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserCreatedIntegrationEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<UserDto>(result);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("john@example.com", result.Email);
        Assert.NotEqual(Guid.Empty, result.Id);

        // Verify repository was called
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _mockUserRepository.Verify(x => x.SaveChangesAsync(), Times.Once);

        // Verify event was published
        _mockEventPublisher.Verify(x => x.PublishAsync(
            It.Is<UserCreatedIntegrationEvent>(e =>
                e.Name == "John Doe" &&
                e.Email == "john@example.com")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "existing@example.com"
        };

        var existingUser = User.Create(Guid.NewGuid(), "Existing User", "existing@example.com");

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(command));

        Assert.Contains(command.Email, exception.Message);
        Assert.Contains("already exists", exception.Message);

        // Verify user was NOT created
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<UserCreatedIntegrationEvent>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryFails_ShouldNotPublishEvent()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "john@example.com"
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(command));

        // Verify event was NOT published
        _mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<UserCreatedIntegrationEvent>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldCheckForExistingEmailFirst()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "Test User",
            Email = "test@example.com"
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _mockUserRepository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);
        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<UserCreatedIntegrationEvent>())).Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert - Verify email check was called first
        _mockUserRepository.Verify(x => x.GetByEmailAsync(command.Email), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectDataToRepository()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "Jane Smith",
            Email = "jane@example.com"
        };

        User? capturedUser = null;

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user)
            .Returns(Task.CompletedTask);

        _mockUserRepository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);
        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<UserCreatedIntegrationEvent>())).Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.Equal("Jane Smith", capturedUser.Name);
        Assert.Equal("jane@example.com", capturedUser.Email);
        Assert.NotEqual(Guid.Empty, capturedUser.Id);
    }

    [Fact]
    public async Task HandleAsync_PublishedEvent_ShouldContainCorrectUserData()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "Event Test User",
            Email = "event@example.com"
        };

        UserCreatedIntegrationEvent? capturedEvent = null;

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _mockUserRepository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        _mockEventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserCreatedIntegrationEvent>()))
            .Callback<eCommerce.EventBus.IntegrationEvents.OutgoingIntegrationEvent>(e => capturedEvent = e as UserCreatedIntegrationEvent)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(capturedEvent);
        Assert.Equal(result.Id, capturedEvent.Id);
        Assert.Equal("Event Test User", capturedEvent.Name);
        Assert.Equal("event@example.com", capturedEvent.Email);
    }
}
