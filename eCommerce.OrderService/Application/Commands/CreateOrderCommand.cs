using eCommerce.Mediator.Commands;
using eCommerce.OrderService.Application.Dtos;

namespace eCommerce.OrderService.Application.Commands;

/// <summary>
/// Command to create a new order.
/// </summary>
public class CreateOrderCommand : ICommand<OrderDto>
{
    public Guid UserId { get; set; }
    public string Product { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
