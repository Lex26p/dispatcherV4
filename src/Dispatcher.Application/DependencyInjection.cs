using Dispatcher.Application.Abstractions;
using Dispatcher.Application.Assets.Equipment;
using Dispatcher.Application.Assets.Locations;
using Microsoft.Extensions.DependencyInjection;

namespace Dispatcher.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDispatcherApplication(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        services.AddScoped<ICurrentUser, AnonymousCurrentUser>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IEquipmentService, EquipmentService>();

        return services;
    }
}
