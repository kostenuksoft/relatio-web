using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Relatio.Contacts.Infrastructure.Data;

public sealed class ContactsDbContextFactory : IDesignTimeDbContextFactory<ContactsDbContext>
{
    public ContactsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Set environment variable 'ConnectionStrings__DefaultConnection'.");

        var optionsBuilder = new DbContextOptionsBuilder<ContactsDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ContactsDbContext(optionsBuilder.Options);
    }
}
