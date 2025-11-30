namespace eCommerce.UserService.Domain.AggregatesModel.UserAggregate;

/// <summary>
/// Repository interface for User aggregate.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task<bool> SaveChangesAsync();
}
