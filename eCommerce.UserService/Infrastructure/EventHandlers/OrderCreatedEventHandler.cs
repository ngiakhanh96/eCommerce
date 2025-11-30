using eCommerce.EventBus.EventHandler;
using eCommerce.UserService.Domain.References;
using eCommerce.UserService.Infrastructure.IntegrationEvents.Incoming;

namespace eCommerce.UserService.Infrastructure.EventHandlers;

/// <summary>
/// Handler for OrderCreatedEvent from OrderService.
/// Persists the order reference in the RefOrder table.
/// </summary>
public class OrderCreatedEventHandler : BaseEventHandler<OrderCreatedIntegrationEvent>
{
    private readonly IRefOrderRepository _refOrderRepository;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger, IRefOrderRepository refOrderRepository)
    {
        _logger = logger;
        _refOrderRepository = refOrderRepository;
    }

    /// <summary>
    /// Handles OrderCreatedEvent by creating a RefOrder record.
    /// </summary>
    protected override async Task HandleImplAsync(OrderCreatedIntegrationEvent? eventDto)
    {
        try
        {
            if (eventDto == null)
            {
                _logger.LogWarning("Failed to deserialize OrderCreatedEvent");
                return;
            }

            // Check if RefOrder already exists
            var existingOrder = await _refOrderRepository.GetByIdAsync(eventDto.Id);
            if (existingOrder != null)
            {
                _logger.LogInformation($"RefOrder with ID {eventDto.Id} already exists, skipping creation");
                return;
            }

            // Create RefOrder
            var refOrder = new RefOrder
            {
                Id = eventDto.Id,
                UserId = eventDto.UserId,
                Product = eventDto.Product,
                Quantity = eventDto.Quantity,
                Price = eventDto.Price,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _refOrderRepository.AddAsync(refOrder);
            await _refOrderRepository.SaveChangesAsync();

            _logger.LogInformation($"RefOrder created successfully with ID: {refOrder.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling OrderCreatedEvent: {ex.Message}", ex);
            throw;
        }
    }
}
