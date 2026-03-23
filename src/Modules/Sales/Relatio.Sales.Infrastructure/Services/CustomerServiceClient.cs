using ErrorOr;
using Microsoft.Extensions.Logging;
using Relatio.Sales.Application.Interfaces;
using Relatio.Sales.Domain.Errors;

namespace Relatio.Sales.Infrastructure.Services;

public sealed class CustomerServiceClient : ICustomerServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerServiceClient> _logger;

    public CustomerServiceClient(HttpClient httpClient, ILogger<CustomerServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ErrorOr<bool>> CustomerExistsAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/customers/{customerId}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
                return true;

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false;

            _logger.LogWarning(
                "Customer service returned unexpected status {StatusCode} for customer {CustomerId}",
                (int)response.StatusCode, customerId);

            return DealErrors.CustomerServiceUnavailable;
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken != cancellationToken)
        {
            _logger.LogWarning("Customer service timed out for customer {CustomerId}", customerId);
            return DealErrors.CustomerServiceUnavailable;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Customer service unavailable for customer {CustomerId}", customerId);
            return DealErrors.CustomerServiceUnavailable;
        }
    }
}
