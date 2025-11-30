using eCommerce.Mediator.Queries;
using eCommerce.UserService.Application.Dtos;

namespace eCommerce.UserService.Application.Queries;

/// <summary>
/// Query to retrieve a user orders by ID.
/// </summary>
public class GetUserOrdersQuery : IQuery<List<OrderDto>>
{
    public Guid UserId { get; }

    public GetUserOrdersQuery(Guid userId)
    {
        UserId = userId;
    }
}
