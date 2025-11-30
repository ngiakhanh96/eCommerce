namespace eCommerce.UserService.Domain.References;

/// <summary>
/// Reference order entity representing an order in the UserService database.
/// This entity is populated from OrderCreatedEvent published by OrderService.
/// Note: This is NOT an aggregate root - it's a read-only reference model that mirrors
/// data from another bounded context (OrderService) for query purposes.
/// </summary>
public class RefOrder
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Product { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
