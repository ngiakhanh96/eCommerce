using eCommerce.EventBus.Publisher;
using eCommerce.EventBus.Resilience;
using eCommerce.EventBus.Subscriber;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eCommerce.EventBus.Extensions;

/// <summary>
/// Extension methods for registering event handling services.
/// </summary>
public static class EventBusServiceExtensions
{
    /// <summary>
    /// Adds Kafka event subscriber and handlers to the dependency injection container.
    /// </summary>
    public static IHostApplicationBuilder AddKafkaEventSubscribers(
        this IHostApplicationBuilder builder,
        string groupId,
        string connectionName,
        Dictionary<string, Type> topicToEventHandlerMapping)
    {
        builder.AddKafkaConsumer<string, string>(connectionName, settings =>
        {
            settings.Config.GroupId = groupId;
            settings.DisableTracing = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddSource(KafkaEventSubscriber.ActivitySourceName);
            });

        // Register Kafka event handlers and event subscriber
        foreach (var (topic, eventHandlerType) in topicToEventHandlerMapping)
        {
            builder.Services.AddScoped(eventHandlerType);
        }
        builder.Services.AddSingleton<IEventSubscriber, KafkaEventSubscriber>(svp =>
            new KafkaEventSubscriber(topicToEventHandlerMapping, svp));

        // Register hosted service to manage the event subscriber lifecycle
        builder.Services.AddHostedService<EventSubscriberHostedService>();

        return builder;
    }

    /// <summary>
    /// Adds Kafka event publisher with resilience policies (retry + circuit breaker) to the dependency injection container.
    /// </summary>
    public static IHostApplicationBuilder AddKafkaEventPublisher(
        this IHostApplicationBuilder builder,
        string connectionName,
        bool enableResilience = true)
    {
        builder.AddKafkaProducer<string, string>(connectionName);
        
        if (enableResilience)
        {
            // Register resilient publisher that wraps the base Kafka publisher
            builder.Services.AddSingleton<KafkaEventPublisher>();
            builder.Services.AddSingleton<IEventPublisher>(sp =>
            {
                var kafkaPublisher = sp.GetRequiredService<KafkaEventPublisher>();
                var logger = sp.GetRequiredService<ILogger<ResilientEventPublisher>>();
                return new ResilientEventPublisher(kafkaPublisher, logger);
            });
        }
        else
        {
            builder.Services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
        }
        
        return builder;
    }
}