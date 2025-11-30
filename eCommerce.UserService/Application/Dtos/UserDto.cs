namespace eCommerce.UserService.Application.Dtos;

/// <summary>
/// Data transfer object for User.
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}