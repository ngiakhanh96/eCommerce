using eCommerce.Mediator.Queries;
using eCommerce.UserService.Application.Dtos;
using eCommerce.UserService.Domain.References;

namespace eCommerce.UserService.Application.Queries;

/// <summary>
/// Query handler for retrieving a user by ID.
/// </summary>
public class GetUserOrdersQueryHandler : IQueryHandler<GetUserOrdersQuery, List<OrderDto>>
{
    private readonly IRefOrderRepository _refOrderRepository;

    public GetUserOrdersQueryHandler(IRefOrderRepository refOrderRepository)
    {
        _refOrderRepository = refOrderRepository;
    }

    public async Task<List<OrderDto>> HandleAsync(GetUserOrdersQuery query)
    {
        var orders = await _refOrderRepository.GetByUserIdAsync(query.UserId);
        return orders.Select(p => new OrderDto
        {
            Id = p.Id,
            UserId = p.UserId,
            Product = p.Product,
            Quantity = p.Quantity,
            Price = p.Price
        }).ToList();
    }
}
