using eCommerce.Mediator.Queries;
using eCommerce.UserService.Application.Dtos;
using eCommerce.UserService.Domain.AggregatesModel.UserAggregate;

namespace eCommerce.UserService.Application.Queries;

/// <summary>
/// Query handler for retrieving a user by ID.
/// </summary>
public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> HandleAsync(GetUserQuery query)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID '{query.UserId}' not found.");
        }

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }
}
