namespace Relatio.Sales.Infrastructure.Settings;

public sealed class KeycloakClientSettings
{
    public string Authority { get; init; } = string.Empty;
    public string ServiceClientId { get; init; } = string.Empty;
    public string ServiceClientSecret { get; init; } = string.Empty;
}
