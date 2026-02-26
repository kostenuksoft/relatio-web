using System.Reflection;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Relatio.Shared.Behaviors;

namespace Relatio.Customers.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomersApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        TypeAdapterConfig.GlobalSettings.Scan(assembly);
        services.TryAddSingleton(TypeAdapterConfig.GlobalSettings);
        services.TryAddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
