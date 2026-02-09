using System.Diagnostics;
using eCommerce.Aop;
using eCommerce.Cache;
using eCommerce.EventBus.Extensions;
using eCommerce.Logging;
using eCommerce.Mediator.Commands;
using eCommerce.Mediator.Extensions;
using eCommerce.Mediator.Queries;
using eCommerce.ServiceDefaults;
using eCommerce.UserService.Application.Commands;
using eCommerce.UserService.Application.Dtos;
using eCommerce.UserService.Application.Queries;
using eCommerce.UserService.Application.Validators;
using eCommerce.UserService.Domain.AggregatesModel.UserAggregate;
using eCommerce.UserService.Domain.References;
using eCommerce.UserService.Infrastructure;
using eCommerce.UserService.Infrastructure.EventHandlers;
using eCommerce.UserService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults(ApplicationConstants.AppName);

// Add EF Core in-memory database
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseInMemoryDatabase("ecommerce-users-db"));

// Add domain services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefOrderRepository, RefOrderRepository>();

// Add Mediator with validation
builder.Services.AddMediatorWithValidation(typeof(CreateUserCommandValidator).Assembly);

// Register command handlers
builder.Services.AddScoped<ICommandHandler<CreateUserCommand, UserDto>, CreateUserCommandHandler>();

// Register query handlers
builder.Services.AddScoped<IQueryHandler<GetUserQuery, UserDto?>, GetUserQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetUserOrdersQuery, List<OrderDto>>, GetUserOrdersQueryHandler>();

// Add Cache service
builder.Services.AddInMemoryCacheProvider();

// Add Kafka event publisher
builder.AddKafkaEventPublisher("kafka");

// Get Kafka consumer group from configuration
var kafkaConsumerGroup = builder.Configuration["Kafka:ConsumerGroup"];

// Add Kafka event subscribers
builder.AddKafkaEventSubscribers(kafkaConsumerGroup, "kafka", new Dictionary<string, Type>
{
    {"order-created", typeof(OrderCreatedEventHandler)}
});

var app = builder
    .WithCacheProxy()
    .WithLogProxy(ApplicationConstants.AppActivitySource)
    .BuildWithProxies();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Create a new user
app.MapPost("/users", async (CreateUserCommand command, ICommandBus commandBus) =>
{
    var userDto = await commandBus.SendAsync(command);
    return Results.Created($"/users/{userDto.Id}", userDto);
})
    .WithName("CreateUser");

// Get user by ID
app.MapGet("/users/{id}", async (Guid id, IQueryBus queryBus) =>
{
    var query = new GetUserQuery(id);
    var user = await queryBus.SendAsync(query);

    return Results.Ok(user);
})
    .WithName("GetUser");

// Get user orders by user ID
app.MapGet("/users/{id}/orders", async (Guid id, IQueryBus queryBus) =>
{
    var query = new GetUserOrdersQuery(id);
    var orders = await queryBus.SendAsync(query);

    return Results.Ok(orders);
})
    .WithName("GetUserOrders");

app.MapDefaultEndpoints();

app.Run();

public static class ApplicationConstants
{
    public static string AppName = "eCommerce.UserService";
    public static ActivitySource AppActivitySource = new(AppName);
}