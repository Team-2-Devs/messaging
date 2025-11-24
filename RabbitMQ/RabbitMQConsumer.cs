using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging.RabbitMQ;

public sealed class RabbitMQConsumer : IEventConsumer
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;
    private string? _queue;

    public RabbitMQConsumer(string host, string user, string pass)
    {
        _factory = new ConnectionFactory
        {
            HostName = host,
            UserName = user,
            Password = pass,
            AutomaticRecoveryEnabled = true
        };
    }

    public async Task SubscribeAsync(string queue, string exchange, CancellationToken ct)
    {
        _connection = await _factory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync();

        // Declare the fanout exchange
        await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Fanout, durable: true, autoDelete: false);

        // Declare and bind the queue
        await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(queue, exchange, "");

        _queue = queue;
    }

    public async Task RunAsync(Func<ReadOnlyMemory<byte>, CancellationToken, Task<bool>> handler, CancellationToken ct)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var ok = await handler(ea.Body, ct);
            if (ok) await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false, ct);
            else await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, ct);
        };

        // Use manual ack since handler returns true or false
        await _channel!.BasicConsumeAsync(_queue!, autoAck: false, consumer, ct);
        await Task.Delay(Timeout.Infinite, ct);
    }

    // public async Task RunAsync(
    // Func<ReadOnlyMemory<byte>, CancellationToken, Task<bool>> handler, CancellationToken ct)
    // {
    //     if (_channel is null)
    //         throw new InvalidOperationException("SubscribeAsync must be called before RunAsync.");

    //     var consumer = new AsyncEventingBasicConsumer(_channel);

    //     consumer.ReceivedAsync += async (_, ea) =>
    //     {
    //         var ok = await handler(ea.Body, ct);

    //         if (ok) await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: ct);
    //         else await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: ct);
    //     };

    //     await _channel.BasicConsumeAsync(queue: _queue!, autoAck: false, consumer: consumer, cancellationToken: ct);

    //     await Task.Delay(Timeout.Infinite, ct);
    // }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.DisposeAsync();
        if (_connection != null) await _connection.DisposeAsync();
    }
}