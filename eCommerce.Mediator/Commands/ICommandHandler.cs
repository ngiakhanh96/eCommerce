namespace eCommerce.Mediator.Commands;

/// <summary>
/// Interface for command handlers.
/// </summary>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command);
}
