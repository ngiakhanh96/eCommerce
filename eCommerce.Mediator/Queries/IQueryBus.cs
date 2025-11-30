namespace eCommerce.Mediator.Queries;

/// <summary>
/// Interface for query bus to dispatch queries to their handlers.
/// </summary>
public interface IQueryBus
{
    /// <summary>
    /// Dispatches a query to its handler and returns the result.
    /// </summary>
    Task<TResult> SendAsync<TResult>(IQuery<TResult> query);
}
