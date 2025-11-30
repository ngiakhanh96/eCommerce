namespace eCommerce.Mediator.Queries;

/// <summary>
/// Interface for query handlers.
/// </summary>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query);
}
