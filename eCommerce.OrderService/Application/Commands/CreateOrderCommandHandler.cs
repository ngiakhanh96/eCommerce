using eCommerce.EventBus.Publisher;
using eCommerce.Mediator.Commands;
using eCommerce.OrderService.Application.Dtos;
using eCommerce.OrderService.Domain.AggregatesModel.OrderAggregate;
using eCommerce.OrderService.Domain.References;
using eCommerce.OrderService.Infrastructure.IntegrationEvents.Outgoing;

namespace eCommerce.OrderService.Application.Commands;

/// <summary>
/// Command handler for creating a new order.
/// </summary>
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRefUserRepository _refUserRepository;
    private readonly IEventPublisher _eventPublisher;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, IRefUserRepository refUserRepository, IEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _refUserRepository = refUserRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<OrderDto> HandleAsync(CreateOrderCommand command)
    {
        // Check if user exists
        var user = await _refUserRepository.GetByIdAsync(command.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID '{command.UserId}' does not exist.");
        }

        // Create order aggregate
        var order = Order.Create(Guid.NewGuid(), command.UserId, command.Product, command.Quantity, command.Price);

        // Persist the order
        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        // Publish domain events
        var orderCreatedEvent =
            new OrderCreatedIntegrationEvent(
                order.Id, 
                order.UserId, 
                order.Product, 
                order.Quantity, 
                order.Price);
        await _eventPublisher.PublishAsync(orderCreatedEvent);

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Product = order.Product,
            Quantity = order.Quantity,
            Price = order.Price
        };
    }
}
