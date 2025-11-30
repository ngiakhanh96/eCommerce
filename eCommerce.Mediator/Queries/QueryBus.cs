namespace eCommerce.Mediator.Queries;

/// <summary>
/// Query bus for dispatching queries to their handlers.
/// </summary>
public class QueryBus : IQueryBus
{
    private readonly IServiceProvider _serviceProvider;

    public QueryBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Dispatches a query to its handler and returns the result.
    /// </summary>
    public async Task<TResult> SendAsync<TResult>(IQuery<TResult> query)
    {
        var queryType = query.GetType();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"Handler not found for query type: {queryType.Name}");

        var handleMethod = handlerType.GetMethod("HandleAsync")
            ?? throw new InvalidOperationException($"HandleAsync method not found on handler: {handlerType.Name}");

        var result = await (Task<TResult>)handleMethod.Invoke(handler, [query])!;
        return result;
    }
}
