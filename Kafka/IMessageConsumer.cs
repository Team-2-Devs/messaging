namespace Messaging.Kafka;

/// <summary>
/// Defines a contract for a Kafka consumer that
/// subscribes to a topic and continuously consumes messages.
/// </summary>
public interface IMessageConsumer : IAsyncDisposable
{
    /// <summary>
    /// Subscribe to a topic. Call once per consumer instance.
    /// </summary>
    /// <param name="topic">The topic to subscribe to.</param>
    void Subscribe(params string[] topics);

    /// <summary>
    /// Starts a background consume loop and invokes the handler
    /// each time a new message is received from Kafka.
    /// Stop by cancelling <paramref name="ct"/>.
    /// </summary>
    /// <param name="handler">
    /// An asynchronous function to handle each consumed message.
    /// The first parameter is the message key, and the second is the message value.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that, when triggered, gracefully stops the consume loop
    /// and closes the consumer connection.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous consume operation.
    /// The task completes when the cancellation token is triggered
    /// or an unrecoverable error occurs.
    /// </returns>
    Task RunAsync(Func<string, string, string, Task> handler, CancellationToken ct);
}