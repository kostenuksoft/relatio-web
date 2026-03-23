using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Relatio.Infrastructure.Messaging;

public sealed class DealCreatedConsumer : BackgroundService
{
    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(10);

    private readonly ConnectionFactory _connectionFactory;
    private readonly RabbitMqConsumerSettings _settings;
    private readonly ILogger<DealCreatedConsumer> _logger;

    public DealCreatedConsumer(
        IOptions<RabbitMqConsumerSettings> settings,
        ILogger<DealCreatedConsumer> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _connectionFactory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DealCreatedConsumer connection lost — reconnecting in {Delay}s", ReconnectDelay.TotalSeconds);
                await Task.Delay(ReconnectDelay, stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _settings.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _settings.DealCreatedDlqQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var queueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", "" },
            { "x-dead-letter-routing-key", _settings.DealCreatedDlqQueue }
        };

        await channel.QueueDeclareAsync(
            queue: _settings.DealCreatedQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArgs,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _settings.DealCreatedQueue,
            exchange: _settings.ExchangeName,
            routingKey: _settings.DealCreatedRoutingKey,
            cancellationToken: cancellationToken);

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(args.Body.Span);
                var message = JsonSerializer.Deserialize<DealCreatedMessageContract>(body);

                if (message is null)
                {
                    _logger.LogWarning("Received null message DeliveryTag={DeliveryTag}", args.DeliveryTag);
                    await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: CancellationToken.None);
                    return;
                }

                _logger.LogInformation(
                    "Deal created event received: DealId={DealId} Title={Title} CustomerId={CustomerId}",
                    message.DealId, message.Title, message.CustomerId);

                await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process DealCreated message DeliveryTag={DeliveryTag}", args.DeliveryTag);
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: CancellationToken.None);
            }
        };

        await channel.BasicConsumeAsync(
            queue: _settings.DealCreatedQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
