namespace Relatio.Sales.Infrastructure.Settings;

public sealed class RabbitMqSettings
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 5672;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string VirtualHost { get; init; } = "/";
    public string ExchangeName { get; init; } = string.Empty;
    public string DealCreatedQueue { get; init; } = string.Empty;
    public string DealCreatedDlqQueue { get; init; } = string.Empty;
    public string DealCreatedRoutingKey { get; init; } = string.Empty;
    public string DealStageChangedQueue { get; init; } = string.Empty;
    public string DealStageChangedDlqQueue { get; init; } = string.Empty;
    public string DealStageChangedRoutingKey { get; init; } = string.Empty;
    public string DealOnboardingTaskCreatedQueue { get; init; } = string.Empty;
    public string DealOnboardingTaskCreatedDlqQueue { get; init; } = string.Empty;
    public string DealOnboardingTaskFailedQueue { get; init; } = string.Empty;
    public string DealOnboardingTaskFailedDlqQueue { get; init; } = string.Empty;
}
