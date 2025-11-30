using eCommerce.OrderService.Domain.References;

namespace eCommerce.OrderService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the RefUser repository.
/// </summary>
public class RefUserRepository : IRefUserRepository
{
    private readonly OrderDbContext _dbContext;

    public RefUserRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RefUser?> GetByIdAsync(Guid id)
    {
        return await _dbContext.RefUsers.FindAsync(id);
    }

    public async Task AddAsync(RefUser refUser)
    {
        await _dbContext.RefUsers.AddAsync(refUser);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync() > 0;
    }
}
