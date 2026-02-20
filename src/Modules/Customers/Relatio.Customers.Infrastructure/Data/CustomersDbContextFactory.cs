using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Relatio.Customers.Infrastructure.Data;

public sealed class CustomersDbContextFactory : IDesignTimeDbContextFactory<CustomersDbContext>
{
    public CustomersDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Set environment variable 'ConnectionStrings__DefaultConnection'.");

        var optionsBuilder = new DbContextOptionsBuilder<CustomersDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new CustomersDbContext(optionsBuilder.Options);
    }
}
