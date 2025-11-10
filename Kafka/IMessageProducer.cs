namespace Messaging;

/// <summary>
/// Defines a contract for producing messages to a message broker.
/// </summary>
public interface IMessageProducer : IAsyncDisposable
{
    /// <summary>
    /// Publishes a message to a specified topic
    /// </summary>
    /// <typeparam name="T">The type of the message payload.</typeparam>
    /// <param name="topic">The name of the Kafka topic to which the message should be published.</param>
    /// <param name="key">Key used for partitioning in Kafka.</param>
    /// <param name="value">The message payload that will be serialized and sent.</param>
    /// <param name="ct">A cancellation token used to abort sending if requested.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous publish operation.
    /// The task completes when Kafka acknowledges that the message has been successfully produced.
    /// </returns>
    Task ProduceAsync<T>(string topic, string key, T value, CancellationToken ct = default);
}