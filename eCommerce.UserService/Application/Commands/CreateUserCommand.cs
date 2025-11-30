using eCommerce.Mediator.Commands;
using eCommerce.UserService.Application.Dtos;

namespace eCommerce.UserService.Application.Commands;

/// <summary>
/// Command to create a new user.
/// </summary>
public class CreateUserCommand : ICommand<UserDto>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
