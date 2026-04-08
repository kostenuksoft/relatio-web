using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Relatio.Tasks.Application.Interfaces;
using Relatio.Tasks.Domain.Entities;
using Relatio.Tasks.Domain.Enums;
using Relatio.Tasks.Domain.Interfaces;
using Relatio.Tasks.Infrastructure.Settings;

namespace Relatio.Tasks.Infrastructure.Messaging;

public sealed class DealWonConsumer : BackgroundService
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DealWonConsumer> _logger;

    public DealWonConsumer(
        RabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqSettings> settings,
        IServiceScopeFactory scopeFactory,
        ILogger<DealWonConsumer> logger)
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
                _logger.LogError(ex, "Tasks consumer - connection error, reconnecting in 5s");
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
            { "x-dead-letter-routing-key", _settings.DealStageChangedDlqQueue }
        };

        await channel.QueueDeclareAsync(
            queue: _settings.DealStageChangedQueue,
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
                var message = JsonSerializer.Deserialize<DealStageChangedMessage>(body);

                if (message is null)
                {
                    _logger.LogWarning("Tasks consumer - received null message, nacking DeliveryTag={DeliveryTag}", ea.DeliveryTag);
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                if (message.NewStage != "ClosedWon")
                {
                    _logger.LogDebug("Tasks consumer - skipping stage {Stage}, not ClosedWon", message.NewStage);
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    return;
                }

                _logger.LogInformation(
                    "Tasks consumer - deal won: DealId={DealId} Title={Title}",
                    message.DealId, message.Title);

                await using var scope = _scopeFactory.CreateAsyncScope();
                var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
                var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<ITasksUnitOfWork>();

                if (message.Title.Contains("[FAIL]", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Tasks consumer - [FAIL] marker detected, emitting failure event for DealId={DealId}", message.DealId);

                    var failurePayload = JsonSerializer.Serialize(new TaskOnboardingFailedMessage(
                        message.DealId,
                        "Task creation intentionally failed due to [FAIL] marker in deal title",
                        DateTimeOffset.UtcNow));

                    await unitOfWork.BeginTransactionAsync(cancellationToken);
                    try
                    {
                        await outboxRepository.AddAsync("task.onboarding.failed", failurePayload, cancellationToken);
                        await unitOfWork.SaveChangesAsync(cancellationToken);
                        await unitOfWork.CommitTransactionAsync(cancellationToken);
                    }
                    catch
                    {
                        await unitOfWork.RollbackTransactionAsync(cancellationToken);
                        throw;
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    return;
                }

                var taskResult = CrmTask.Create(
                    $"Onboarding: {message.Title}",
                    $"Complete onboarding process for deal {message.Title}",
                    DateTimeOffset.UtcNow.AddDays(7),
                    CrmTaskPriority.High,
                    null);

                if (taskResult.IsError)
                {
                    _logger.LogError("Tasks consumer - failed to create task: {Errors}",
                        string.Join(", ", taskResult.Errors.Select(e => e.Code)));
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                var task = taskResult.Value;

                var successPayload = JsonSerializer.Serialize(new TaskOnboardingCreatedMessage(
                    message.DealId,
                    task.Id,
                    DateTimeOffset.UtcNow));

                await unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    await taskRepository.AddAsync(task, cancellationToken);
                    await outboxRepository.AddAsync("task.onboarding.created", successPayload, cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    await unitOfWork.CommitTransactionAsync(cancellationToken);

                    _logger.LogInformation("Tasks consumer - task created: TaskId={TaskId} for DealId={DealId}", task.Id, message.DealId);
                }
                catch
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tasks consumer - failed to process message DeliveryTag={DeliveryTag}", ea.DeliveryTag);
                try { await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false); }
                catch { /* channel may be closed */ }
            }
        };

        await channel.BasicConsumeAsync(
            queue: _settings.DealStageChangedQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Tasks consumer - listening on queue {Queue}", _settings.DealStageChangedQueue);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private sealed record DealStageChangedMessage(Guid DealId, string Title, string PreviousStage, string NewStage, DateTimeOffset OccurredAt);
    private sealed record TaskOnboardingCreatedMessage(Guid DealId, Guid TaskId, DateTimeOffset OccurredAt);
    private sealed record TaskOnboardingFailedMessage(Guid DealId, string Reason, DateTimeOffset OccurredAt);
}
