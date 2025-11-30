using eCommerce.UserService.Domain.References;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.UserService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the RefOrder repository.
/// </summary>
public class RefOrderRepository : IRefOrderRepository
{
    private readonly UserDbContext _dbContext;

    public RefOrderRepository(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RefOrder?> GetByIdAsync(Guid id)
    {
        return await _dbContext.RefOrders.FindAsync(id);
    }

    public async Task<List<RefOrder>> GetByUserIdAsync(Guid id)
    {
        return await _dbContext.RefOrders.Where(p => p.UserId == id).ToListAsync();
    }

    public async Task AddAsync(RefOrder refOrder)
    {
        await _dbContext.RefOrders.AddAsync(refOrder);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync() > 0;
    }
}
