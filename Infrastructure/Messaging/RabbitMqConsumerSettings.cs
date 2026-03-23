namespace Relatio.Infrastructure.Messaging;

public sealed class RabbitMqConsumerSettings
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
}
