using Confluent.Kafka;
using Confluent.Kafka.Admin;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddDockerComposeEnvironment("env");

// Add Kafka resource
var kafka = builder.AddKafka("kafka")
    .WithKafkaUI();

builder.Eventing.Subscribe<ResourceReadyEvent>(kafka.Resource, async (@event, ct) =>
{
    var cs = await kafka.Resource.ConnectionStringExpression.GetValueAsync(ct);

    var config = new AdminClientConfig
    {
        BootstrapServers = cs
    };

    using var adminClient = new AdminClientBuilder(config).Build();
    try
    {
        await adminClient.CreateTopicsAsync([
            new() { Name = "user-created", NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = "order-created", NumPartitions = 1, ReplicationFactor = 1 }
        ]);
    }
    catch (CreateTopicsException e)
    {
        Console.WriteLine($"An error occurred creating topic: {e.Message}");
        throw;
    }
});

// Add UserService
var userService = builder.AddProject<Projects.eCommerce_UserService>("userservice")
    .WithHttpHealthCheck("/health")
    .WithEnvironment("Kafka:ConsumerGroup", "user-service-consumer-group")
    .WithExternalHttpEndpoints()
    .WithReference(kafka)
    .WaitFor(kafka);

// Add OrderService
var orderService = builder.AddProject<Projects.eCommerce_OrderService>("orderservice")
    .WithHttpHealthCheck("/health")
    .WithEnvironment("Kafka:ConsumerGroup", "order-service-consumer-group")
    .WithExternalHttpEndpoints()
    .WithReference(kafka)
    .WaitFor(kafka);

builder.Build().Run();
