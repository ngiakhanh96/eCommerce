using eCommerce.Mediator.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Mediator.Tests.Queries;

/// <summary>
/// Unit tests for QueryBus.
/// </summary>
public class QueryBusTests
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

    public class ListQuery : IQuery<List<int>>
    {
        public int Count { get; set; }
    }

    public class ListQueryHandler : IQueryHandler<ListQuery, List<int>>
    {
        public Task<List<int>> HandleAsync(ListQuery query)
        {
            var result = Enumerable.Range(1, query.Count).ToList();
            return Task.FromResult(result);
        }
    }

    public class NullableQuery : IQuery<string?>
    {
        public bool ReturnNull { get; set; }
    }

    public class NullableQueryHandler : IQueryHandler<NullableQuery, string?>
    {
        public Task<string?> HandleAsync(NullableQuery query)
        {
            return Task.FromResult(query.ReturnNull ? null : "Not Null");
        }
    }

    public class UnregisteredQuery : IQuery<string>
    {
        public string Data { get; set; } = string.Empty;
    }

    #endregion

    [Fact]
    public async Task SendAsync_WithRegisteredHandler_ShouldReturnResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<TestQuery, string>, TestQueryHandler>();
        var serviceProvider = services.BuildServiceProvider();

        var queryBus = new QueryBus(serviceProvider);
        var query = new TestQuery { SearchTerm = "Laptop" };

        // Act
        var result = await queryBus.SendAsync(query);

        // Assert
        Assert.Equal("Found: Laptop", result);
    }

    [Fact]
    public async Task SendAsync_WithUnregisteredHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var queryBus = new QueryBus(serviceProvider);
        var query = new UnregisteredQuery { Data = "Test" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => queryBus.SendAsync(query));

        Assert.Contains("Handler not found", exception.Message);
        Assert.Contains("UnregisteredQuery", exception.Message);
    }

    [Fact]
    public async Task SendAsync_WithListResultType_ShouldReturnCorrectType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<ListQuery, List<int>>, ListQueryHandler>();
        var serviceProvider = services.BuildServiceProvider();

        var queryBus = new QueryBus(serviceProvider);
        var query = new ListQuery { Count = 5 };

        // Act
        var result = await queryBus.SendAsync(query);

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal(new List<int> { 1, 2, 3, 4, 5 }, result);
    }

    [Fact]
    public async Task SendAsync_WithNullableResult_ShouldReturnNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<NullableQuery, string?>, NullableQueryHandler>();
        var serviceProvider = services.BuildServiceProvider();

        var queryBus = new QueryBus(serviceProvider);
        var query = new NullableQuery { ReturnNull = true };

        // Act
        var result = await queryBus.SendAsync(query);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SendAsync_WithNullableResult_ShouldReturnValue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<NullableQuery, string?>, NullableQueryHandler>();
        var serviceProvider = services.BuildServiceProvider();

        var queryBus = new QueryBus(serviceProvider);
        var query = new NullableQuery { ReturnNull = false };

        // Act
        var result = await queryBus.SendAsync(query);

        // Assert
        Assert.Equal("Not Null", result);
    }

    [Fact]
    public async Task SendAsync_ShouldResolveHandlerFromServiceProvider()
    {
        // Arrange
        var mockHandler = new Mock<IQueryHandler<TestQuery, string>>();
        mockHandler
            .Setup(x => x.HandleAsync(It.IsAny<TestQuery>()))
            .ReturnsAsync("Mocked Result");

        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<TestQuery, string>>(_ => mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var queryBus = new QueryBus(serviceProvider);
        var query = new TestQuery { SearchTerm = "Test" };

        // Act
        var result = await queryBus.SendAsync(query);

        // Assert
        Assert.Equal("Mocked Result", result);
        mockHandler.Verify(x => x.HandleAsync(query), Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldPassQueryToHandler()
    {
        // Arrange
        TestQuery? capturedQuery = null;
        var mockHandler = new Mock<IQueryHandler<TestQuery, string>>();
        mockHandler
            .Setup(x => x.HandleAsync(It.IsAny<TestQuery>()))
            .Callback<TestQuery>(q => capturedQuery = q)
            .ReturnsAsync("Result");

        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<TestQuery, string>>(_ => mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var queryBus = new QueryBus(serviceProvider);
        var query = new TestQuery { SearchTerm = "Captured Term" };

        // Act
        await queryBus.SendAsync(query);

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Equal("Captured Term", capturedQuery.SearchTerm);
    }
}
