using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Kafka;

/// <summary>
/// Dependency injection helpers for registering Kafka messaging components.
/// </summary>
public static class MessagingRegistration
{
    /// <summary>
    /// Registers <see cref="KafkaOptions"/>, <see cref="IMessageProducer"/>,
    /// and optionally <see cref="IMessageConsumer"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="clientId">Client identifier, appears in Kafka logs/metrics.</param>
    /// <param name="groupId">
    /// Optional consumer group id. If provided, an <see cref="IMessageConsumer"/> will be registered.
    /// If omitted or null, only the producer will be registered.
    /// </param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddKafkaMessaging(
        this IServiceCollection services,
        string clientId,
        string? groupId = null)
    {
        var bootstrap = Environment.GetEnvironmentVariable("KAFKA_BROKERS");
        var saslUser = Environment.GetEnvironmentVariable("KAFKA_SASL_USERNAME");
        var saslPass = Environment.GetEnvironmentVariable("KAFKA_SASL_PASSWORD");

        var opts = new KafkaOptions
        {
            BootstrapServers = bootstrap,
            ClientId = clientId,
            SaslUsername = string.IsNullOrWhiteSpace(saslUser) ? null : saslUser,
            SaslPassword = string.IsNullOrWhiteSpace(saslPass) ? null : saslPass,
            SecurityProtocol = string.IsNullOrWhiteSpace(saslUser) ? "PLAINTEXT" : "SASL_SSL"
        };

        services.AddSingleton(opts);
        services.AddSingleton<IMessageProducer, KafkaProducer>();

        if (!string.IsNullOrWhiteSpace(groupId))
        {
            services.AddSingleton<IMessageConsumer>(sp =>
                new KafkaConsumer(sp.GetRequiredService<KafkaOptions>(), groupId));
        }

        return services;
    }
}