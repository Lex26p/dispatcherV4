using Dispatcher.Application.Abstractions;
using Dispatcher.Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;

namespace Dispatcher.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDispatcherInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}
