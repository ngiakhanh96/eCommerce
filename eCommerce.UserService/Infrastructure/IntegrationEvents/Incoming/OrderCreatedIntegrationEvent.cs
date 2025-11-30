namespace eCommerce.UserService.Infrastructure.IntegrationEvents.Incoming;

/// <summary>
/// Represents the OrderCreatedEvent payload received from OrderService.
/// </summary>
public class OrderCreatedIntegrationEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Product { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
