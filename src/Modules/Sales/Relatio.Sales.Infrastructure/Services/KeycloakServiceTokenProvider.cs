using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Relatio.Sales.Application.Interfaces;
using Relatio.Sales.Infrastructure.Settings;

namespace Relatio.Sales.Infrastructure.Services;

public sealed class KeycloakServiceTokenProvider : IServiceTokenProvider, IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly KeycloakClientSettings _settings;
    private readonly ILogger<KeycloakServiceTokenProvider> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _cachedToken;
    private DateTimeOffset _tokenExpiresAt = DateTimeOffset.MinValue;

    public KeycloakServiceTokenProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<KeycloakClientSettings> settings,
        ILogger<KeycloakServiceTokenProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiresAt - TimeSpan.FromSeconds(30))
            return _cachedToken;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiresAt - TimeSpan.FromSeconds(30))
                return _cachedToken;

            return await FetchTokenAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<string> FetchTokenAsync(CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient("keycloak-token");

        var tokenUrl = $"{_settings.Authority.TrimEnd('/')}/protocol/openid-connect/token";

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _settings.ServiceClientId),
            new KeyValuePair<string, string>("client_secret", _settings.ServiceClientSecret)
        });

        var response = await client.PostAsync(tokenUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(json)
            ?? throw new InvalidOperationException("Failed to deserialize Keycloak token response.");

        _cachedToken = tokenResponse.AccessToken;
        _tokenExpiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(tokenResponse.ExpiresIn);

        _logger.LogInformation("Service token acquired, expires at {ExpiresAt}", _tokenExpiresAt);

        return _cachedToken;
    }

    public void Dispose() => _lock.Dispose();

    private sealed record KeycloakTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
