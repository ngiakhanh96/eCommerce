namespace eCommerce.OrderService.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// Repository interface for Order aggregate.
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Order order);
    Task<bool> SaveChangesAsync();
}
