using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Relatio.Contacts.Infrastructure.Settings;
using System.Text;
using System.Text.Json;

namespace Relatio.Contacts.Infrastructure.Messaging;

public sealed class DealCreatedConsumer : BackgroundService
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<DealCreatedConsumer> _logger;

    public DealCreatedConsumer(
        RabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqSettings> settings,
        ILogger<DealCreatedConsumer> logger)
    {
        _connectionFactory = connectionFactory;
        _settings = settings.Value;
        _logger = logger;
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
                _logger.LogError(ex, "Contacts consumer - connection error, reconnecting in 5s");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        var connection = await _connectionFactory.GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

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

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.Span);
                var message = JsonSerializer.Deserialize<DealCreatedMessage>(body);

                if (message is null)
                {
                    _logger.LogWarning("Contacts consumer - received null message, nacking DeliveryTag={DeliveryTag}", ea.DeliveryTag);
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                _logger.LogInformation(
                    "Contacts consumer - deal created: DealId={DealId} Title={Title} Amount={Amount} {Currency} Stage={Stage} CustomerId={CustomerId} OccurredAt={OccurredAt}",
                    message.DealId, message.Title, message.Amount, message.Currency,
                    message.Stage, message.CustomerId, message.OccurredAt);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Contacts consumer - failed to process message DeliveryTag={DeliveryTag}", ea.DeliveryTag);
                try { await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false); }
                catch { /* channel may be closed */ }
            }
        };

        await channel.BasicConsumeAsync(
            queue: _settings.DealCreatedQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Contacts consumer - listening on queue {Queue}", _settings.DealCreatedQueue);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
