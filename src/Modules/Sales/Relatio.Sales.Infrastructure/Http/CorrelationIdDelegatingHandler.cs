using Microsoft.AspNetCore.Http;
using Relatio.Sales.Application.Interfaces;

namespace Relatio.Sales.Infrastructure.Http;

public sealed class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceTokenProvider _serviceTokenProvider;

    public CorrelationIdDelegatingHandler(
        IHttpContextAccessor httpContextAccessor,
        IServiceTokenProvider serviceTokenProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceTokenProvider = serviceTokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items[CorrelationIdHeader]?.ToString()
            ?? Guid.CreateVersion7().ToString();

        request.Headers.TryAddWithoutValidation(CorrelationIdHeader, correlationId);

        var token = await _serviceTokenProvider.GetTokenAsync(cancellationToken);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
