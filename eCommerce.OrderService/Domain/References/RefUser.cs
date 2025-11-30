namespace eCommerce.OrderService.Domain.References;

/// <summary>
/// Reference user entity representing a user in the OrderService database.
/// This entity is populated from UserCreatedEvent published by UserService.
/// Note: This is NOT an aggregate root - it's a read-only reference model that mirrors
/// data from another bounded context (UserService) for query purposes.
/// </summary>
public class RefUser
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
