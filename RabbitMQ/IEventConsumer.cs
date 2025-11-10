namespace Messaging.RabbitMQ;

/// <summary>
/// Defines a consumer for events that subscribes to an exchange
/// and processes domain events published by other services.
/// </summary>
public interface IEventConsumer : IAsyncDisposable
{
    /// <summary>
    /// Subscribes to a RabbitMQ exchange by declaring the exchange and queue,
    /// and binding them together.
    /// </summary>
    /// <typeparam name="queue">Name of the queue that will receive events.</typeparam>
    /// <param name="exchange">Name of the exchange to subscribe to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous subscription setup operation.</returns>
    Task SubscribeAsync(string queue, string exchange, CancellationToken ct);

    /// <summary>
    /// Starts consuming events from the subscribed queue and invokes the specified handler
    /// for each received event payload.
    /// </summary>
    /// <typeparam name="handler">Delegate that processes each message payload.</typeparam>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that represents the lifetime of the consume loop.</returns>
    Task RunAsync(Func<ReadOnlyMemory<byte>, CancellationToken, Task<bool>> handler, CancellationToken ct);
}