namespace eCommerce.OrderService.Domain.References;

/// <summary>
/// Repository interface for RefUser operations.
/// </summary>
public interface IRefUserRepository
{
    Task<RefUser?> GetByIdAsync(Guid id);
    Task AddAsync(RefUser refUser);
    Task<bool> SaveChangesAsync();
}
