using eCommerce.EventBus.IntegrationEvents;

namespace eCommerce.OrderService.Infrastructure.IntegrationEvents.Outgoing;

/// <summary>
/// Order created event raised when a new order is created.
/// </summary>
public class OrderCreatedIntegrationEvent : OutgoingIntegrationEvent
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public string Product { get; }
    public int Quantity { get; }
    public decimal Price { get; }

    public OrderCreatedIntegrationEvent(Guid orderId, Guid userId, string product, int quantity, decimal price) : base("order-created")
    {
        Id = orderId;
        UserId = userId;
        Product = product;
        Quantity = quantity;
        Price = price;
    }
}
