using Confluent.Kafka;

namespace Messaging.Kafka;

public class KafkaConsumer : IMessageConsumer
{
    private readonly IConsumer<string, string> _consumer;
    private string[]? _topics;

    public KafkaConsumer(KafkaOptions opts, string groupId)
    {
        var cfg = new ConsumerConfig
        {
            BootstrapServers = opts.BootstrapServers,
            GroupId = groupId,
            ClientId = opts.ClientId,
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = true,
            EnablePartitionEof = false
        };

        if (!string.IsNullOrWhiteSpace(opts.SaslUsername))
        {
            cfg.SecurityProtocol = SecurityProtocol.SaslSsl;
            cfg.SaslMechanism = SaslMechanism.ScramSha256;
            cfg.SaslUsername = opts.SaslUsername;
            cfg.SaslPassword = opts.SaslPassword;
        }

        _consumer = new ConsumerBuilder<string, string>(cfg).Build();
    }

    public void Subscribe(params string[] topics)
    {
        if (topics == null || topics.Length == 0)
            throw new ArgumentException("At least one topic", nameof(topics));

        _topics = topics;
        _consumer.Subscribe(topics);
    }

    public async Task RunAsync(Func<string, string, string, Task> handler, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var cr = _consumer.Consume(ct);
                    if (cr == null || cr.IsPartitionEOF)
                        continue;

                    var topic = cr.Topic;
                    var key = cr.Message.Key ?? string.Empty;
                    var value = cr.Message.Value ?? string.Empty;

                    await handler(topic, key, value);
                }
                catch (ConsumeException ex)
                {
                    if (ex.Error.Code is ErrorCode.UnknownTopicOrPart or ErrorCode.Local_UnknownTopic)
                    {
                        // Topic may not exist yet, or broker just restarted.
                        await Task.Delay(TimeSpan.FromSeconds(2), ct);

                        if (_topics is { Length: > 0 })
                        {
                            try
                            {
                                _consumer.Subscribe(_topics);
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        continue;
                    }

                    // Other transient error
                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // graceful shutdown
        }
        finally
        {
            try { _consumer.Close(); } catch { }
        }
    }

    public ValueTask DisposeAsync()
    {
        try { _consumer.Close(); } catch { }
        _consumer.Dispose();
        return ValueTask.CompletedTask;
    }
}