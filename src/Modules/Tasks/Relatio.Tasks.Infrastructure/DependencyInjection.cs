using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Relatio.Tasks.Domain.Interfaces;
using Relatio.Tasks.Infrastructure.Data;
using Relatio.Tasks.Infrastructure.Repositories;

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

        return services;
    }
}
