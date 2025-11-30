using eCommerce.Mediator.Queries;
using eCommerce.Mediator.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Mediator.Tests.Queries;

/// <summary>
/// Unit tests for ValidatingQueryBus.
/// </summary>
public class ValidatingQueryBusTests
{
    #region Test Queries and Handlers

    public class TestQuery : IQuery<string>
    {
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<string> HandleAsync(TestQuery query)
        {
            return Task.FromResult($"Found: {query.SearchTerm}");
        }
    }

    public class TestQueryValidator : AbstractValidator<TestQuery>
    {
        public TestQueryValidator()
        {
            RuleFor(x => x.SearchTerm).NotEmpty().WithMessage("SearchTerm is required.");
        }
    }

    public class MultipleErrorsQuery : IQuery<string>
    {
        public string Field1 { get; set; } = string.Empty;
        public string Field2 { get; set; } = string.Empty;
    }

    public class MultipleErrorsQueryValidator : AbstractValidator<MultipleErrorsQuery>
    {
        public MultipleErrorsQueryValidator()
        {
            RuleFor(x => x.Field1).NotEmpty().WithMessage("Field1 is required.");
            RuleFor(x => x.Field2).NotEmpty().WithMessage("Field2 is required.");
        }
    }

    public class MultipleErrorsQueryHandler : IQueryHandler<MultipleErrorsQuery, string>
    {
        public Task<string> HandleAsync(MultipleErrorsQuery query)
        {
            return Task.FromResult($"{query.Field1} {query.Field2}");
        }
    }

    #endregion

    [Fact]
    public async Task SendAsync_WithValidQuery_ShouldReturnResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<TestQuery, string>, TestQueryHandler>();
        services.AddScoped<IValidator<TestQuery>, TestQueryValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var innerBus = new QueryBus(serviceProvider);
        var validatingBus = new ValidatingQueryBus(innerBus, serviceProvider);

        var query = new TestQuery { SearchTerm = "Test Search" };

        // Act
        var result = await validatingBus.SendAsync(query);

        // Assert
        Assert.Equal("Found: Test Search", result);
    }

    [Fact]
    public async Task SendAsync_WithInvalidQuery_ShouldThrowRequestValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<TestQuery, string>, TestQueryHandler>();
        services.AddScoped<IValidator<TestQuery>, TestQueryValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var innerBus = new QueryBus(serviceProvider);
        var validatingBus = new ValidatingQueryBus(innerBus, serviceProvider);

        var query = new TestQuery { SearchTerm = "" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestValidationException>(
            () => validatingBus.SendAsync(query));

        Assert.Contains("SearchTerm", exception.Errors.Keys);
        Assert.Contains("SearchTerm is required.", exception.Errors["SearchTerm"]);
    }

    [Fact]
    public async Task SendAsync_WithNoValidator_ShouldProceedWithoutValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<TestQuery, string>, TestQueryHandler>();
        // No validator registered
        var serviceProvider = services.BuildServiceProvider();

        var innerBus = new QueryBus(serviceProvider);
        var validatingBus = new ValidatingQueryBus(innerBus, serviceProvider);

        var query = new TestQuery { SearchTerm = "" }; // Would fail validation if validator existed

        // Act
        var result = await validatingBus.SendAsync(query);

        // Assert
        Assert.Equal("Found: ", result);
    }

    [Fact]
    public async Task SendAsync_WithMultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<MultipleErrorsQuery, string>, MultipleErrorsQueryHandler>();
        services.AddScoped<IValidator<MultipleErrorsQuery>, MultipleErrorsQueryValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var innerBus = new QueryBus(serviceProvider);
        var validatingBus = new ValidatingQueryBus(innerBus, serviceProvider);

        var query = new MultipleErrorsQuery { Field1 = "", Field2 = "" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestValidationException>(
            () => validatingBus.SendAsync(query));

        Assert.Equal(2, exception.Errors.Count);
        Assert.Contains("Field1", exception.Errors.Keys);
        Assert.Contains("Field2", exception.Errors.Keys);
    }

    [Fact]
    public async Task SendAsync_ShouldValidateBeforeCallingHandler()
    {
        // Arrange
        var mockHandler = new Mock<IQueryHandler<TestQuery, string>>();
        mockHandler
            .Setup(x => x.HandleAsync(It.IsAny<TestQuery>()))
            .ReturnsAsync("Result");

        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<TestQuery, string>>(_ => mockHandler.Object);
        services.AddScoped<IValidator<TestQuery>, TestQueryValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var innerBus = new QueryBus(serviceProvider);
        var validatingBus = new ValidatingQueryBus(innerBus, serviceProvider);

        var query = new TestQuery { SearchTerm = "" };

        // Act & Assert
        await Assert.ThrowsAsync<RequestValidationException>(
            () => validatingBus.SendAsync(query));

        // Handler should never be called if validation fails
        mockHandler.Verify(x => x.HandleAsync(It.IsAny<TestQuery>()), Times.Never);
    }
}
