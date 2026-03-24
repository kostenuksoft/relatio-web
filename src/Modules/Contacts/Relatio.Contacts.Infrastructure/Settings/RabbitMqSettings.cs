namespace Relatio.Contacts.Infrastructure.Settings;

public sealed class RabbitMqSettings
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 5672;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string VirtualHost { get; init; } = "/";
    public string DealCreatedQueue { get; init; } = string.Empty;
    public string DealCreatedDlqQueue { get; init; } = string.Empty;
}
