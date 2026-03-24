using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Relatio.Contacts.Domain.Interfaces;
using Relatio.Contacts.Infrastructure.Data;
using Relatio.Contacts.Infrastructure.Messaging;
using Relatio.Contacts.Infrastructure.Repositories;
using Relatio.Contacts.Infrastructure.Settings;

namespace Relatio.Contacts.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddContactsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ContactsDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IContactsUnitOfWork>(provider => provider.GetRequiredService<ContactsDbContext>());
        services.AddScoped<IContactRepository, ContactRepository>();

        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddHostedService<DealCreatedConsumer>();

        return services;
    }
}
