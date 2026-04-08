using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Relatio.Tasks.Infrastructure.Data;
using Relatio.Tasks.Infrastructure.Outbox;
using Relatio.Tasks.Infrastructure.Settings;

namespace Relatio.Tasks.Infrastructure.Messaging;

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
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(stoppingToken);
            await Task.Delay(PollingInterval, stoppingToken);
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
            _logger.LogWarning(ex, "Tasks relay - RabbitMQ unavailable, skipping outbox relay cycle");
            return;
        }

        await using var _ = channel;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();

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
            _logger.LogError(ex, "Tasks relay - failed to query outbox messages");
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
                    "task.onboarding.created" => _settings.OnboardingTaskCreatedRoutingKey,
                    "task.onboarding.failed" => _settings.OnboardingTaskFailedRoutingKey,
                    _ => _settings.OnboardingTaskCreatedRoutingKey
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
                _logger.LogWarning(ex, "Tasks relay - failed to publish outbox message {MessageId}", message.Id);
                message.MarkFailed(ex.Message, MaxRetries);
            }
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tasks relay - failed to save outbox relay results");
        }
    }
}
