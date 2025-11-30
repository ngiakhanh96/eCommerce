using System.Text.Json;

namespace eCommerce.EventBus.EventHandler;

public abstract class BaseEventHandler<TEvent>
{
    public async Task HandleAsync(string eventJson)
    {
        var eventDto = JsonSerializer.Deserialize<TEvent>(eventJson);
        await HandleImplAsync(eventDto);
    }

    protected abstract Task HandleImplAsync(TEvent? @event);

}