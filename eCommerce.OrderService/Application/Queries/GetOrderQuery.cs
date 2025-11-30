using eCommerce.Mediator.Queries;
using eCommerce.OrderService.Application.Dtos;

namespace eCommerce.OrderService.Application.Queries;

/// <summary>
/// Query to retrieve an order by ID.
/// </summary>
public class GetOrderQuery : IQuery<OrderDto?>
{
    public Guid OrderId { get; }

    public GetOrderQuery(Guid orderId)
    {
        OrderId = orderId;
    }
}
