namespace eCommerce.OrderService.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// Order aggregate root implementing DDD principles.
/// </summary>
public class Order : AggregateRoot
{
    // Private constructor for EF Core
    private Order() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Product { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Factory method to create a new Order aggregate.
    /// </summary>
    public static Order Create(Guid id, Guid userId, string product, int quantity, decimal price)
    {
        var order = new Order
        {
            Id = id,
            UserId = userId,
            Product = product,
            Quantity = quantity,
            Price = price,
            CreatedAt = DateTime.UtcNow
        };

        return order;
    }
}
