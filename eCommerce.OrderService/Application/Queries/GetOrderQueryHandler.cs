using eCommerce.Mediator.Queries;
using eCommerce.OrderService.Application.Dtos;
using eCommerce.OrderService.Domain.AggregatesModel.OrderAggregate;

namespace eCommerce.OrderService.Application.Queries;

/// <summary>
/// Query handler for retrieving an order by ID.
/// </summary>
public class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> HandleAsync(GetOrderQuery query)
    {
        var order = await _orderRepository.GetByIdAsync(query.OrderId);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID '{query.OrderId}' not found.");
        }

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Product = order.Product,
            Quantity = order.Quantity,
            Price = order.Price
        };
    }
}
