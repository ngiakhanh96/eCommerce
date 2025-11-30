using eCommerce.EventBus.Extensions;
using eCommerce.Mediator.Commands;
using eCommerce.Mediator.Extensions;
using eCommerce.Mediator.Queries;
using eCommerce.OrderService.Application.Commands;
using eCommerce.OrderService.Application.Dtos;
using eCommerce.OrderService.Application.Queries;
using eCommerce.OrderService.Application.Validators;
using eCommerce.OrderService.Domain.AggregatesModel.OrderAggregate;
using eCommerce.OrderService.Domain.References;
using eCommerce.OrderService.Infrastructure;
using eCommerce.OrderService.Infrastructure.EventHandlers;
using eCommerce.OrderService.Infrastructure.Repositories;
using eCommerce.ServiceDefaults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add EF Core in-memory database
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("ecommerce-orders-db"));

// Add repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IRefUserRepository, RefUserRepository>();

// Add Mediator with validation
builder.Services.AddMediatorWithValidation(typeof(CreateOrderCommandValidator).Assembly);

// Register command handlers
builder.Services.AddScoped<ICommandHandler<CreateOrderCommand, OrderDto>, CreateOrderCommandHandler>();

// Register query handlers
builder.Services.AddScoped<IQueryHandler<GetOrderQuery, OrderDto?>, GetOrderQueryHandler>();

// Add Kafka event publisher
builder.AddKafkaEventPublisher("kafka");

// Get Kafka consumer group from configuration
var kafkaConsumerGroup = builder.Configuration["Kafka:ConsumerGroup"];

// Add Kafka event subscribers
builder.AddKafkaEventSubscribers(kafkaConsumerGroup, "kafka", new Dictionary<string, Type>
{
    {"user-created", typeof(UserCreatedEventHandler)}
});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Create a new order
app.MapPost("/orders", async (CreateOrderCommand command, ICommandBus commandBus) =>
{
    var orderDto = await commandBus.SendAsync(command);
    return Results.Created($"/orders/{orderDto.Id}", orderDto);
})
    .WithName("CreateOrder");

// Get order by ID
app.MapGet("/orders/{id}", async (Guid id, IQueryBus queryBus) =>
{
    var query = new GetOrderQuery(id);
    var order = await queryBus.SendAsync(query);

    return Results.Ok(order);
})
    .WithName("GetOrder");

app.MapDefaultEndpoints();

app.Run();