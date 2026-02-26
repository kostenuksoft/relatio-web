using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Relatio.Customers.Domain.Interfaces;
using Relatio.Customers.Infrastructure.Data;
using Relatio.Customers.Infrastructure.Repositories;

namespace Relatio.Customers.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<CustomersDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICustomersUnitOfWork>(provider => provider.GetRequiredService<CustomersDbContext>());
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        return services;
    }
}
