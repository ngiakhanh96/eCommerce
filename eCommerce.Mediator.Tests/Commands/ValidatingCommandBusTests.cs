using eCommerce.Mediator.Commands;
using eCommerce.Mediator.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Mediator.Tests.Commands;

/// <summary>
/// Unit tests for ValidatingCommandBus.
/// </summary>
public class ValidatingCommandBusTests
{
    #region Test Commands and Handlers

    public class TestCommand : ICommand<string>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<string> HandleAsync(TestCommand command)
        {
            return Task.FromResult($"Handled: {command.Name}");
        }
    }

    public class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        }
    }

    public class MultipleErrorsCommand : ICommand<string>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class MultipleErrorsCommandValidator : AbstractValidator<MultipleErrorsCommand>
    {
        public MultipleErrorsCommandValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("FirstName is required.");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("LastName is required.");
        }
    }

    public class MultipleErrorsCommandHandler : ICommandHandler<MultipleErrorsCommand, string>
    {
        public Task<string> HandleAsync(MultipleErrorsCommand command)
        {
            return Task.FromResult($"{command.FirstName} {command.LastName}");
        }
    }

    #endregion

    [Fact]
    public async Task SendAsync_WithValidCommand_ShouldReturnResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommand, string>, TestCommandHandler>();
        services.AddScoped<IValidator<TestCommand>, TestCommandValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var innerBus = new CommandBus(serviceProvider);
        var validatingBus = new ValidatingCommandBus(innerBus, serviceProvider);

        var command = new TestCommand { Name = "Test Name" };

        // Act
        var result = await validatingBus.SendAsync(command);

        // Assert
        Assert.Equal("Handled: Test Name", result);
    }

    [Fact]
    public async Task SendAsync_WithInvalidCommand_ShouldThrowRequestValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommand, string>, TestCommandHandler>();
        services.AddScoped<IValidator<TestCommand>, TestCommandValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var innerBus = new CommandBus(serviceProvider);
        var validatingBus = new ValidatingCommandBus(innerBus, serviceProvider);

        var command = new TestCommand { Name = "" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestValidationException>(
            () => validatingBus.SendAsync(command));

        Assert.Contains("Name", exception.Errors.Keys);
        Assert.Contains("Name is required.", exception.Errors["Name"]);
    }

    [Fact]
    public async Task SendAsync_WithNoValidator_ShouldProceedWithoutValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommand, string>, TestCommandHandler>();
        // No validator registered
        var serviceProvider = services.BuildServiceProvider();

        var innerBus = new CommandBus(serviceProvider);
        var validatingBus = new ValidatingCommandBus(innerBus, serviceProvider);

        var command = new TestCommand { Name = "" }; // Would fail validation if validator existed

        // Act
        var result = await validatingBus.SendAsync(command);

        // Assert
        Assert.Equal("Handled: ", result);
    }

    [Fact]
    public async Task SendAsync_WithMultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<MultipleErrorsCommand, string>, MultipleErrorsCommandHandler>();
        services.AddScoped<IValidator<MultipleErrorsCommand>, MultipleErrorsCommandValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var innerBus = new CommandBus(serviceProvider);
        var validatingBus = new ValidatingCommandBus(innerBus, serviceProvider);

        var command = new MultipleErrorsCommand { FirstName = "", LastName = "" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestValidationException>(
            () => validatingBus.SendAsync(command));

        Assert.Equal(2, exception.Errors.Count);
        Assert.Contains("FirstName", exception.Errors.Keys);
        Assert.Contains("LastName", exception.Errors.Keys);
    }

    [Fact]
    public async Task SendAsync_ShouldValidateBeforeCallingHandler()
    {
        // Arrange
        var mockHandler = new Mock<ICommandHandler<TestCommand, string>>();
        mockHandler
            .Setup(x => x.HandleAsync(It.IsAny<TestCommand>()))
            .ReturnsAsync("Result");

        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommand, string>>(_ => mockHandler.Object);
        services.AddScoped<IValidator<TestCommand>, TestCommandValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var innerBus = new CommandBus(serviceProvider);
        var validatingBus = new ValidatingCommandBus(innerBus, serviceProvider);

        var command = new TestCommand { Name = "" };

        // Act & Assert
        await Assert.ThrowsAsync<RequestValidationException>(
            () => validatingBus.SendAsync(command));

        // Handler should never be called if validation fails
        mockHandler.Verify(x => x.HandleAsync(It.IsAny<TestCommand>()), Times.Never);
    }
}
