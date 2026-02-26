using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Relatio.Shared.Behaviors;

namespace Relatio.Contacts.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddContactsApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

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
