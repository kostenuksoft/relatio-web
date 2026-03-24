using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Relatio.Sales.Application.Interfaces;
using Relatio.Sales.Domain.Interfaces;
using Relatio.Sales.Infrastructure.Data;
using Relatio.Sales.Infrastructure.Http;
using Relatio.Sales.Infrastructure.Messaging;
using Relatio.Sales.Infrastructure.Repositories;
using Relatio.Sales.Infrastructure.Services;
using Relatio.Sales.Infrastructure.Settings;

namespace Relatio.Sales.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<SalesDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ISalesUnitOfWork>(provider => provider.GetRequiredService<SalesDbContext>());
        services.AddScoped<IDealRepository, DealRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddHostedService<OutboxRelayService>();

        services.Configure<KeycloakClientSettings>(configuration.GetSection("Keycloak"));
        services.AddHttpClient("keycloak-token");
        services.AddSingleton<IServiceTokenProvider, KeycloakServiceTokenProvider>();

        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdDelegatingHandler>();

        var customersBaseUrl = configuration["Services:CustomersBaseUrl"]
            ?? throw new InvalidOperationException("Services:CustomersBaseUrl is not configured.");

        services.AddHttpClient<ICustomerServiceClient, CustomerServiceClient>(client =>
        {
            client.BaseAddress = new Uri(customersBaseUrl);
        })
        .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
        .AddStandardResilienceHandler(options =>
        {
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(8);
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromMilliseconds(300);
            options.Retry.UseJitter = true;
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.MinimumThroughput = 5;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
