using eCommerce.Mediator.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Mediator.Tests.Commands;

/// <summary>
/// Unit tests for CommandBus.
/// </summary>
public class CommandBusTests
{
    #region Test Commands and Handlers

    public class TestCommand : ICommand<string>
    {
        public string Input { get; set; } = string.Empty;
    }

    public class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<string> HandleAsync(TestCommand command)
        {
            return Task.FromResult($"Handled: {command.Input}");
        }
    }

    public class IntResultCommand : ICommand<int>
    {
        public int Value { get; set; }
    }

    public class IntResultCommandHandler : ICommandHandler<IntResultCommand, int>
    {
        public Task<int> HandleAsync(IntResultCommand command)
        {
            return Task.FromResult(command.Value * 2);
        }
    }

    public class UnregisteredCommand : ICommand<string>
    {
        public string Data { get; set; } = string.Empty;
    }

    #endregion

    [Fact]
    public async Task SendAsync_WithRegisteredHandler_ShouldReturnResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommand, string>, TestCommandHandler>();
        var serviceProvider = services.BuildServiceProvider();

        var commandBus = new CommandBus(serviceProvider);
        var command = new TestCommand { Input = "Hello World" };

        // Act
        var result = await commandBus.SendAsync(command);

        // Assert
        Assert.Equal("Handled: Hello World", result);
    }

    [Fact]
    public async Task SendAsync_WithUnregisteredHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var commandBus = new CommandBus(serviceProvider);
        var command = new UnregisteredCommand { Data = "Test" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => commandBus.SendAsync(command));

        Assert.Contains("Handler not found", exception.Message);
        Assert.Contains("UnregisteredCommand", exception.Message);
    }

    [Fact]
    public async Task SendAsync_WithDifferentResultType_ShouldReturnCorrectType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<IntResultCommand, int>, IntResultCommandHandler>();
        var serviceProvider = services.BuildServiceProvider();

        var commandBus = new CommandBus(serviceProvider);
        var command = new IntResultCommand { Value = 21 };

        // Act
        var result = await commandBus.SendAsync(command);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task SendAsync_ShouldResolveHandlerFromServiceProvider()
    {
        // Arrange
        var mockHandler = new Mock<ICommandHandler<TestCommand, string>>();
        mockHandler
            .Setup(x => x.HandleAsync(It.IsAny<TestCommand>()))
            .ReturnsAsync("Mocked Result");

        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommand, string>>(_ => mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var commandBus = new CommandBus(serviceProvider);
        var command = new TestCommand { Input = "Test" };

        // Act
        var result = await commandBus.SendAsync(command);

        // Assert
        Assert.Equal("Mocked Result", result);
        mockHandler.Verify(x => x.HandleAsync(command), Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldPassCommandToHandler()
    {
        // Arrange
        TestCommand? capturedCommand = null;
        var mockHandler = new Mock<ICommandHandler<TestCommand, string>>();
        mockHandler
            .Setup(x => x.HandleAsync(It.IsAny<TestCommand>()))
            .Callback<TestCommand>(cmd => capturedCommand = cmd)
            .ReturnsAsync("Result");

        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommand, string>>(_ => mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var commandBus = new CommandBus(serviceProvider);
        var command = new TestCommand { Input = "Captured Input" };

        // Act
        await commandBus.SendAsync(command);

        // Assert
        Assert.NotNull(capturedCommand);
        Assert.Equal("Captured Input", capturedCommand.Input);
    }
}
