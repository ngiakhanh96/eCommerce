namespace eCommerce.OrderService.Infrastructure.IntegrationEvents.Incoming;

/// <summary>
/// Represents the UserCreatedEvent payload received from UserService.
/// </summary>
public class UserCreatedIntegrationEvent
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}