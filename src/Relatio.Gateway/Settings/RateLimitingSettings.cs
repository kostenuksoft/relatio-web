namespace Relatio.Gateway.Settings;

public sealed class RateLimitingSettings
{
    public int RequestsPerWindow { get; init; } = 30;
    public int WindowSeconds { get; init; } = 60;
}
