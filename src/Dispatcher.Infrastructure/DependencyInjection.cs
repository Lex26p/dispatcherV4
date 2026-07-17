using Dispatcher.Application.Abstractions.Persistence;
using Dispatcher.Application.Abstractions.System;
using Dispatcher.Infrastructure.Persistence;
using Dispatcher.Infrastructure.Persistence.Repositories;
using Dispatcher.Infrastructure.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dispatcher.Infrastructure;

public static class DependencyInjection
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Password=postgres";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DispatcherDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = DefaultConnectionString;
        }

        services.AddDbContext<DispatcherDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<DispatcherDbContext>());
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITagValueRepository, TagValueRepository>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
