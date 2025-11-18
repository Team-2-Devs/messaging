namespace Messaging.Kafka;

public class KafkaOptions
{
    public string BootstrapServers { get; init; } =
        Environment.GetEnvironmentVariable("KAFKA_BROKERS") ?? "kafka:29092";

    public string ClientId { get; init; } =
        Environment.GetEnvironmentVariable("KAFKA_CLIENT_ID")!;

    public string? SaslUsername { get; init; } // Use later
    public string? SaslPassword { get; init; } // Use later

    public string SecurityProtocol { get; init; } = "PLAINTEXT"; // No encryption/auth. Later: SASL_SSL => encrypted and authenticated
    public string? Acks { get; init; }
}