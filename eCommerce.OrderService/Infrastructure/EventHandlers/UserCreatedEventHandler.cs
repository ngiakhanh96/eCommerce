using eCommerce.EventBus.EventHandler;
using eCommerce.OrderService.Domain.References;
using eCommerce.OrderService.Infrastructure.IntegrationEvents.Incoming;

namespace eCommerce.OrderService.Infrastructure.EventHandlers;

/// <summary>
/// Handler for UserCreatedEvent from UserService.
/// Persists the user reference in the RefUser table.
/// </summary>
public class UserCreatedEventHandler : BaseEventHandler<UserCreatedIntegrationEvent>
{
    private readonly IRefUserRepository _refUserRepository;
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger, IRefUserRepository refUserRepository)
    {
        _logger = logger;
        _refUserRepository = refUserRepository;
    }

    /// <summary>
    /// Handles UserCreatedEvent by creating a RefUser record.
    /// </summary>
    protected override async Task HandleImplAsync(UserCreatedIntegrationEvent? eventDto)
    {
        try
        {
            if (eventDto == null)
            {
                _logger.LogWarning("Failed to deserialize UserCreatedEvent");
                return;
            }

            // Check if RefUser already exists
            var existingUser = await _refUserRepository.GetByIdAsync(eventDto.Id);
            if (existingUser != null)
            {
                _logger.LogInformation($"RefUser with ID {eventDto.Id} already exists, skipping creation");
                return;
            }

            // Create RefUser
            var refUser = new RefUser
            {
                Id = eventDto.Id,
                Name = eventDto.Name,
                Email = eventDto.Email,
                CreatedAt = DateTime.UtcNow
            };

            await _refUserRepository.AddAsync(refUser);
            await _refUserRepository.SaveChangesAsync();

            _logger.LogInformation($"RefUser created successfully with ID: {refUser.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling UserCreatedEvent: {ex.Message}", ex);
            throw;
        }
    }
}
