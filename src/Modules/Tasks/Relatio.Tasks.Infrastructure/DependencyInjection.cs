using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Relatio.Tasks.Application.Interfaces;
using Relatio.Tasks.Domain.Interfaces;
using Relatio.Tasks.Infrastructure.Data;
using Relatio.Tasks.Infrastructure.Messaging;
using Relatio.Tasks.Infrastructure.Repositories;
using Relatio.Tasks.Infrastructure.Settings;

namespace Relatio.Tasks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTasksInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<TasksDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITasksUnitOfWork>(provider => provider.GetRequiredService<TasksDbContext>());
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddHostedService<OutboxRelayService>();
        services.AddHostedService<DealWonConsumer>();

        return services;
    }
}
