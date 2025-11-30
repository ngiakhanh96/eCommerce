namespace eCommerce.Mediator.Commands;

/// <summary>
/// Interface for command bus to dispatch commands to their handlers.
/// </summary>
public interface ICommandBus
{
    /// <summary>
    /// Dispatches a command to its handler and returns the result.
    /// </summary>
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command);
}
