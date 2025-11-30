namespace eCommerce.OrderService.Application.Dtos;

/// <summary>
/// Data transfer object for Order.
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Product { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
