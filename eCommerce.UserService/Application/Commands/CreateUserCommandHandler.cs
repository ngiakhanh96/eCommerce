using eCommerce.EventBus.Publisher;
using eCommerce.Logging;
using eCommerce.Mediator.Commands;
using eCommerce.UserService.Application.Dtos;
using eCommerce.UserService.Domain.AggregatesModel.UserAggregate;
using eCommerce.UserService.Infrastructure.IntegrationEvents.Outgoing;

namespace eCommerce.UserService.Application.Commands;

/// <summary>
/// Command handler for creating a new user.
/// </summary>
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventPublisher _eventPublisher;

    public CreateUserCommandHandler(IUserRepository userRepository, IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
    }

    [Log]
    public async Task<UserDto> HandleAsync(CreateUserCommand command)
    {
        // Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(command.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"A user with email '{command.Email}' already exists.");
        }

        // Create user aggregate
        var user = User.Create(Guid.NewGuid(), command.Name, command.Email);

        // Persist the user
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Publish domain events
        var userCreatedEvent =
            new UserCreatedIntegrationEvent(
                user.Id, 
                user.Name, 
                user.Email);
        await _eventPublisher.PublishAsync(userCreatedEvent);


        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }
}
