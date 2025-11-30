using Microsoft.EntityFrameworkCore;
using eCommerce.OrderService.Domain.AggregatesModel.OrderAggregate;

namespace eCommerce.OrderService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the Order repository.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _dbContext;

    public OrderRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Orders.FindAsync(id);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
    {
        return await _dbContext.Orders
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(Order order)
    {
        await _dbContext.Orders.AddAsync(order);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync() > 0;
    }
}
