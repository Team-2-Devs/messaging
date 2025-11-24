using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Messaging.RabbitMQ;

/// <summary>
/// RabbitMQ publisher that ensures a single shared connection and
/// publishes events to configured exchanges.
/// Implements <see cref="IEventPublisher"/> so services can interact with RabbitMQ without transport details.
/// </summary>
public sealed class RabbitMQPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly string _host, _user, _pass;
    private readonly int _port;
    private IConnection? _connection;
    private static readonly JsonSerializerOptions _jsonOpts = new(JsonSerializerDefaults.Web);

    // Ensures that only one caller at a time can initialize the connection
    private readonly SemaphoreSlim _initLock = new(1, 1);

    /// <summary>
    /// Creates a new RabbitMQ publisher with credentials and hostname.
    /// </summary>
    public RabbitMQPublisher(string host, string user, string pass)
    : this(host, user, pass, 5672) { }

    /// <summary>
    /// Creates a new RabbitMQ publisher with credentials and hostname and port.
    /// </summary>
    public RabbitMQPublisher(string host, string user, string pass, int port)
        => (_host, _user, _pass, _port) = (host, user, pass, port);

    /// <summary>
    /// Lazily initializes the RabbitMQ connection in a thread-safe manner.
    /// Ensures only one connection attempt happens at a time.
    /// </summary>
    private async Task EnsureConnectionAsync(CancellationToken ct)
    {
        // Already connected and open
        if (_connection is { IsOpen: true }) return;

        await _initLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_connection is { IsOpen: true }) return;

            var factory = new ConnectionFactory {
                HostName = _host,
                UserName = _user,
                Password = _pass,
                Port = _port,
                AutomaticRecoveryEnabled = true
            };

            // Establish new connection
            _connection = await factory.CreateConnectionAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task PublishAsync<T>(string exchange, string routingKey, T @event,
        ExchangeKind kind = ExchangeKind.Fanout, CancellationToken ct = default)
    {
        Console.WriteLine($"[RabbitMQPublisher] PublishAsync called. " +
                        $"exchange={exchange}, routingKey={routingKey}, kind={kind}, " +
                        $"host={_host}, port={_port}, user={_user}");

        try
        {
            await EnsureConnectionAsync(ct).ConfigureAwait(false);
            Console.WriteLine("[RabbitMQPublisher] Connection ensured. IsOpen=" + _connection!.IsOpen);

            await using var channel = await _connection!.CreateChannelAsync().ConfigureAwait(false);
            Console.WriteLine("[RabbitMQPublisher] Channel opened. ChannelNumber=" + channel.ChannelNumber);

            var type = kind.ToString().ToLowerInvariant();
            Console.WriteLine($"[RabbitMQPublisher] Declaring exchange '{exchange}' type='{type}'");

            await channel.ExchangeDeclareAsync(exchange, type: type, durable: true, autoDelete: false, arguments: null)
                .ConfigureAwait(false);

            var payloadJson = JsonSerializer.Serialize(@event, _jsonOpts);
            var payload = Encoding.UTF8.GetBytes(payloadJson);
            Console.WriteLine($"[RabbitMQPublisher] Payload length={payload.Length} bytes");

            var props = new BasicProperties { ContentType = "application/json", DeliveryMode = DeliveryModes.Persistent };

            await channel.BasicPublishAsync<BasicProperties>(
                    exchange, routingKey, mandatory: false, basicProperties: props, body: payload, cancellationToken: ct)
                .ConfigureAwait(false);

            Console.WriteLine("[RabbitMQPublisher] ✅ BasicPublishAsync completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[RabbitMQPublisher] ❌ Publish failed:");
            Console.WriteLine(ex);
            throw;
        }
    }
    /// <summary>
    /// Disposes the RabbitMQ connection and associated resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync().ConfigureAwait(false);

        _initLock.Dispose();
    }
}