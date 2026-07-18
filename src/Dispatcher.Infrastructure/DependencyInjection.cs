using Dispatcher.Application.Abstractions;
using Dispatcher.Infrastructure.Persistence;
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

        return services;
    }
}
