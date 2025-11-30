using eCommerce.Mediator.Queries;
using eCommerce.UserService.Application.Dtos;

namespace eCommerce.UserService.Application.Queries;

/// <summary>
/// Query to retrieve a user by ID.
/// </summary>
public class GetUserQuery : IQuery<UserDto?>
{
    public Guid UserId { get; }

    public GetUserQuery(Guid userId)
    {
        UserId = userId;
    }
}
