namespace eCommerce.UserService.Domain.References;

/// <summary>
/// Repository interface for RefOrder operations.
/// </summary>
public interface IRefOrderRepository
{
    Task<RefOrder?> GetByIdAsync(Guid id);
    Task<List<RefOrder>> GetByUserIdAsync(Guid id);
    Task AddAsync(RefOrder refOrder);
    Task<bool> SaveChangesAsync();
}
