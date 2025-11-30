namespace eCommerce.UserService.Domain.AggregatesModel.UserAggregate;

/// <summary>
/// User aggregate root implementing DDD principles.
/// </summary>
public class User : AggregateRoot
{
    // Private constructor for EF Core
    private User() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Factory method to create a new User aggregate.
    /// </summary>
    public static User Create(Guid id, string name, string email)
    {
        var user = new User
        {
            Id = id,
            Name = name,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        return user;
    }
}
