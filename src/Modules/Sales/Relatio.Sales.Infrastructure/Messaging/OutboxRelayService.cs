using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Relatio.Sales.Infrastructure.Data;
using Relatio.Sales.Infrastructure.Outbox;
using Relatio.Sales.Infrastructure.Settings;
using System.Text;

namespace Relatio.Sales.Infrastructure.Messaging;

public sealed class OutboxRelayService : BackgroundService
{
    private const int BatchSize = 50;
    private const int MaxRetries = 5;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<OutboxRelayService> _logger;

    public OutboxRelayService(
        IServiceScopeFactory scopeFactory,
        RabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqSettings> settings,
        ILogger<OutboxRelayService> logger)
    {
        _scopeFactory = scopeFactory;
        _connectionFactory = connectionFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DeclareTopologyAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(stoppingToken);
            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task DeclareTopologyAsync(CancellationToken cancellationToken)
    {
        try
        {
            var connection = await _connectionFactory.GetConnectionAsync(cancellationToken);
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

            await channel.QueueDeclareAsync(
                queue: _settings.DealStageChangedDlqQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            var stageChangedQueueArgs = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", "" },
                { "x-dead-letter-routing-key", _settings.DealStageChangedDlqQueue }
            };

            await channel.QueueDeclareAsync(
                queue: _settings.DealStageChangedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: stageChangedQueueArgs,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: _settings.DealStageChangedQueue,
                exchange: _settings.ExchangeName,
                routingKey: _settings.DealStageChangedRoutingKey,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: _settings.DealOnboardingTaskCreatedDlqQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            var onboardingCreatedQueueArgs = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", "" },
                { "x-dead-letter-routing-key", _settings.DealOnboardingTaskCreatedDlqQueue }
            };

            await channel.QueueDeclareAsync(
                queue: _settings.DealOnboardingTaskCreatedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: onboardingCreatedQueueArgs,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: _settings.DealOnboardingTaskFailedDlqQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            var onboardingFailedQueueArgs = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", "" },
                { "x-dead-letter-routing-key", _settings.DealOnboardingTaskFailedDlqQueue }
            };

            await channel.QueueDeclareAsync(
                queue: _settings.DealOnboardingTaskFailedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: onboardingFailedQueueArgs,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: _settings.DealOnboardingTaskCreatedQueue,
                exchange: _settings.ExchangeName,
                routingKey: "task.onboarding.created",
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: _settings.DealOnboardingTaskFailedQueue,
                exchange: _settings.ExchangeName,
                routingKey: "task.onboarding.failed",
                cancellationToken: cancellationToken);

            _logger.LogInformation("RabbitMQ topology declared successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to declare RabbitMQ topology — relay will not start");
            throw;
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        IChannel? channel = null;

        try
        {
            var connection = await _connectionFactory.GetConnectionAsync(cancellationToken);
            channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ unavailable — skipping outbox relay cycle");
            return;
        }

        await using var _ = channel;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SalesDbContext>();

        List<OutboxMessage> messages;
        try
        {
            messages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedAt == null && m.RetryCount < MaxRetries)
                .OrderBy(m => m.CreatedAt)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query outbox messages");
            return;
        }

        if (messages.Count == 0)
            return;

        foreach (var message in messages)
        {
            try
            {
                var properties = new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent,
                    MessageId = message.Id.ToString(),
                    Type = message.EventType
                };

                var body = Encoding.UTF8.GetBytes(message.Payload);

                var routingKey = message.EventType switch
                {
                    "deal.created" => _settings.DealCreatedRoutingKey,
                    "deal.stage.changed" => _settings.DealStageChangedRoutingKey,
                    _ => _settings.DealCreatedRoutingKey
                };

                await channel.BasicPublishAsync(
                    exchange: _settings.ExchangeName,
                    routingKey: routingKey,
                    mandatory: false,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: cancellationToken);

                message.MarkProcessed();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish outbox message {MessageId}", message.Id);
                message.MarkFailed(ex.Message, MaxRetries);
            }
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save outbox relay results");
        }
    }
}
