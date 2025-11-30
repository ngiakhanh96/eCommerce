namespace eCommerce.Mediator.Commands;

/// <summary>
/// Base interface for commands.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Base interface for commands that return a result.
/// </summary>
public interface ICommand<TResult> : ICommand
{
}
