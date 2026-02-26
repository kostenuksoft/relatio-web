using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Relatio.Sales.Domain.Interfaces;
using Relatio.Sales.Infrastructure.Data;
using Relatio.Sales.Infrastructure.Repositories;

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

        return services;
    }
}
