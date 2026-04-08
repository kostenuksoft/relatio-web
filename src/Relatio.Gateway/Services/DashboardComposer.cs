using System.Text.Json;
using Microsoft.Extensions.Options;
using Relatio.Gateway.Models;

namespace Relatio.Gateway.Services;

public sealed class DashboardComposer
{
    private readonly HttpClient _httpClient;
    private readonly ServicesConfiguration _services;
    private readonly ILogger<DashboardComposer> _logger;

    public DashboardComposer(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<DashboardComposer> logger)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
        _services = configuration.GetSection("Services").Get<ServicesConfiguration>()
            ?? throw new InvalidOperationException("Services configuration not found");
        _logger = logger;
    }

    public async Task<CustomerDashboardResponse> GetCustomerDashboardAsync(
        Guid customerId,
        string? authorizationHeader,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var unavailable = new List<string>();

        var customerTask = FetchAsync<CustomerDto>(
            $"{_services.Customers}/api/customers/{customerId}",
            "customers",
            authorizationHeader,
            correlationId,
            unavailable,
            cancellationToken);

        var contactsTask = FetchAsync<PagedContactsDto>(
            $"{_services.Contacts}/api/contacts?customerId={customerId}&pageSize=10",
            "contacts",
            authorizationHeader,
            correlationId,
            unavailable,
            cancellationToken);

        var dealsTask = FetchAsync<PagedDealsDto>(
            $"{_services.Sales}/api/deals?customerId={customerId}&pageSize=10",
            "deals",
            authorizationHeader,
            correlationId,
            unavailable,
            cancellationToken);

        var tasksTask = FetchAsync<PagedTasksDto>(
            $"{_services.Tasks}/api/tasks?pageSize=10",
            "tasks",
            authorizationHeader,
            correlationId,
            unavailable,
            cancellationToken);

        await Task.WhenAll(customerTask, contactsTask, dealsTask, tasksTask);

        return new CustomerDashboardResponse
        {
            Customer = customerTask.Result,
            Contacts = contactsTask.Result,
            Deals = dealsTask.Result,
            Tasks = tasksTask.Result,
            Unavailable = unavailable
        };
    }

    private async Task<T?> FetchAsync<T>(
        string url,
        string serviceName,
        string? authorizationHeader,
        string? correlationId,
        List<string> unavailable,
        CancellationToken cancellationToken) where T : class
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (!string.IsNullOrEmpty(authorizationHeader))
                request.Headers.Add("Authorization", authorizationHeader);

            request.Headers.Add("api-version", "1.0");

            if (!string.IsNullOrEmpty(correlationId))
                request.Headers.Add("X-Correlation-Id", correlationId);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Gateway - {Service} returned {StatusCode}", serviceName, response.StatusCode);
                unavailable.Add(serviceName);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway - failed to fetch from {Service}", serviceName);
            unavailable.Add(serviceName);
            return null;
        }
    }
}

public sealed class ServicesConfiguration
{
    public string Identity { get; init; } = string.Empty;
    public string Customers { get; init; } = string.Empty;
    public string Contacts { get; init; } = string.Empty;
    public string Sales { get; init; } = string.Empty;
    public string Tasks { get; init; } = string.Empty;
}
