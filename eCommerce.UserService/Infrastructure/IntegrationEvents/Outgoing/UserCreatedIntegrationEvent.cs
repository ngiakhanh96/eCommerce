using eCommerce.EventBus.IntegrationEvents;

namespace eCommerce.UserService.Infrastructure.IntegrationEvents.Outgoing;

/// <summary>
/// User created event raised when a new user is created.
/// </summary>
public class UserCreatedIntegrationEvent : OutgoingIntegrationEvent
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public UserCreatedIntegrationEvent(Guid id, string name, string email) : base("user-created")
    {
        Id = id;
        Name = name;
        Email = email;
    }
}