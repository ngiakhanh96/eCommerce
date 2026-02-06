using Confluent.Kafka;
using eCommerce.EventBus.EventHandler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace eCommerce.EventBus.Subscriber;

/// <summary>
/// Kafka implementation of the event subscriber.
/// Consumes events from Kafka topics and delegates to appropriate handlers.
/// </summary>
public class KafkaEventSubscriber : IEventSubscriber
{
    public static readonly string ActivitySourceName = "KafkaConsumer";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaEventSubscriber> _logger;
    private Task? _consumerTask;
    private readonly Dictionary<string, Type> _topicToEventHandlerMapping;
    public KafkaEventSubscriber(
        Dictionary<string, Type> topicToEventHandlerMapping, 
        IServiceProvider serviceProvider)
    {
        _topicToEventHandlerMapping = topicToEventHandlerMapping;
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetRequiredService<ILogger<KafkaEventSubscriber>>();
        _consumer = _serviceProvider.GetRequiredService<IConsumer<string, string>>();
    }

    /// <summary>
    /// Starts consuming events from Kafka topics.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_topicToEventHandlerMapping.Keys);

        _logger.LogInformation("Kafka event subscriber started ...");

        _consumerTask = Task.Run(() => ConsumeMessages(cancellationToken), cancellationToken);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops consuming events from Kafka.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Kafka event subscriber...");

        if (_consumerTask != null)
        {
            await _consumerTask;
        }

        _consumer?.Close();
        _consumer?.Dispose();

        _logger.LogInformation("Kafka event subscriber stopped.");
    }

    /// <summary>
    /// Consumes messages from Kafka and routes to appropriate handlers.
    /// </summary>
    private async Task ConsumeMessages(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = _consumer.Consume(cancellationToken);

                if (message == null)
                    continue;

                _logger.LogInformation($"Received message from topic '{message.Topic}': {message.Message.Value}");
                var headerBytes = message.Message.Headers.GetLastBytes("traceparent");
                var parentId = headerBytes != null ? Encoding.UTF8.GetString(headerBytes) : null;
                // 4. Start the Activity manually
                using var activity = ActivitySource.StartActivity(
                    "kafka.consume", 
                    ActivityKind.Consumer, 
                    parentId); // This links it to the Producer!

                activity?.SetTag("messaging.kafka.topic", message.Topic);
                try
                {
                    // Route to appropriate handler based on topic
                    await RouteMessageAsync(message.Topic, message.Message.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing message from topic '{message.Topic}': {ex.Message}", ex);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in Kafka consumer: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Routes messages to the appropriate handler based on the topic.
    /// </summary>
    private async Task RouteMessageAsync(string topic, string message)
    {
        using var scope = _serviceProvider.CreateScope();
        if (_topicToEventHandlerMapping.TryGetValue(topic, out var eventHandlerType))
        {
            var userCreatedHandler = scope.ServiceProvider.GetRequiredService(eventHandlerType);
            // Get the HandleAsync method
            var handleAsyncMethod = eventHandlerType.GetMethod(nameof(BaseEventHandler<>.HandleAsync));
            if (handleAsyncMethod != null)
            {
                // Invoke the method
                var task = handleAsyncMethod.Invoke(userCreatedHandler, [message]);
                // If it returns a Task, await it
                if (task is Task taskResult)
                {
                    await taskResult;
                }
            }
        }
        else
        {
            _logger.LogWarning($"Unknown topic: {topic}");
        }
    }
}
