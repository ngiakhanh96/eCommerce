namespace eCommerce.Mediator.Commands;

/// <summary>
/// Command bus for dispatching commands to their handlers.
/// </summary>
public class CommandBus : ICommandBus
{
    private readonly IServiceProvider _serviceProvider;

    public CommandBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Dispatches a command to its handler and returns the result.
    /// </summary>
    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command)
    {
        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"Handler not found for command type: {commandType.Name}");

        var handleMethod = handlerType.GetMethod("HandleAsync")
            ?? throw new InvalidOperationException($"HandleAsync method not found on handler: {handlerType.Name}");

        var result = await (Task<TResult>)handleMethod.Invoke(handler, [command])!;
        return result;
    }
}
