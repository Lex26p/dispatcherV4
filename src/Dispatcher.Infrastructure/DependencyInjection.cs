using Dispatcher.Application.Abstractions;
using Dispatcher.Application.Assets.Equipment;
using Dispatcher.Application.Assets.Locations;
using Dispatcher.Application.Telemetry.Configuration;
using Dispatcher.Infrastructure.Assets;
using Dispatcher.Infrastructure.Persistence;
using Dispatcher.Infrastructure.Telemetry;
using Dispatcher.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dispatcher.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDispatcherInfrastructure(this IServiceCollection services, string dispatcherDatabaseConnectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dispatcherDatabaseConnectionString);

        services.AddSingleton<IClock, SystemClock>();
        services.AddDbContext<DispatcherDbContext>(options => options.UseNpgsql(dispatcherDatabaseConnectionString));
        services.AddScoped<ILocationRepository, EfLocationRepository>();
        services.AddScoped<IEquipmentRepository, EfEquipmentRepository>();
        services.AddScoped<ITelemetryConfigurationRepository, EfTelemetryConfigurationRepository>();

        return services;
    }
}
