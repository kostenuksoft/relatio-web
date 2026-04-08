using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Relatio.Sales.Application.Commands.ChangeDealStage;
using Relatio.Sales.Infrastructure.Settings;

namespace Relatio.Sales.Infrastructure.Messaging;

public sealed class DealSagaConsumer : BackgroundService
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DealSagaConsumer> _logger;

    public DealSagaConsumer(
        RabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqSettings> settings,
        IServiceScopeFactory scopeFactory,
        ILogger<DealSagaConsumer> logger)
    {
        _connectionFactory = connectionFactory;
        _settings = settings.Value;
        _scopeFactory = scopeFactory;
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
                _logger.LogError(ex, "Sales saga consumer - connection error, reconnecting in 5s");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        var connection = await _connectionFactory.GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: cancellationToken);

        var successConsumer = new AsyncEventingBasicConsumer(channel);
        successConsumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.Span);
                var message = JsonSerializer.Deserialize<TaskOnboardingCreatedMessage>(body);

                if (message is null)
                {
                    _logger.LogWarning("Sales saga consumer - received null message, nacking DeliveryTag={DeliveryTag}", ea.DeliveryTag);
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                _logger.LogInformation(
                    "Sales saga consumer - task onboarding created: DealId={DealId} TaskId={TaskId}",
                    message.DealId, message.TaskId);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sales saga consumer - failed to process success message DeliveryTag={DeliveryTag}", ea.DeliveryTag);
                try { await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false); }
                catch { /* channel may be closed */ }
            }
        };

        var failureConsumer = new AsyncEventingBasicConsumer(channel);
        failureConsumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.Span);
                var message = JsonSerializer.Deserialize<TaskOnboardingFailedMessage>(body);

                if (message is null)
                {
                    _logger.LogWarning("Sales saga consumer - received null message, nacking DeliveryTag={DeliveryTag}", ea.DeliveryTag);
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                _logger.LogWarning(
                    "Sales saga consumer - task onboarding failed: DealId={DealId} Reason={Reason} - compensating saga",
                    message.DealId, message.Reason);

                await using var scope = _scopeFactory.CreateAsyncScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                var compensateResult = await sender.Send(new ChangeDealStageCommand(message.DealId, "Negotiation"), cancellationToken);

                if (compensateResult.IsError)
                {
                    _logger.LogError("Sales saga consumer - compensation failed for DealId={DealId}: {Errors}",
                        message.DealId, string.Join(", ", compensateResult.Errors.Select(e => e.Code)));
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                    return;
                }

                _logger.LogInformation("Sales saga consumer - compensation successful: DealId={DealId} reverted to Negotiation", message.DealId);
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sales saga consumer - failed to process failure message DeliveryTag={DeliveryTag}", ea.DeliveryTag);
                try { await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false); }
                catch { /* channel may be closed */ }
            }
        };

        await channel.BasicConsumeAsync(
            queue: _settings.DealOnboardingTaskCreatedQueue,
            autoAck: false,
            consumer: successConsumer,
            cancellationToken: cancellationToken);

        await channel.BasicConsumeAsync(
            queue: _settings.DealOnboardingTaskFailedQueue,
            autoAck: false,
            consumer: failureConsumer,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Sales saga consumer - listening on {SuccessQueue} and {FailureQueue}",
            _settings.DealOnboardingTaskCreatedQueue, _settings.DealOnboardingTaskFailedQueue);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private sealed record TaskOnboardingCreatedMessage(Guid DealId, Guid TaskId, DateTimeOffset OccurredAt);
    private sealed record TaskOnboardingFailedMessage(Guid DealId, string Reason, DateTimeOffset OccurredAt);
}
