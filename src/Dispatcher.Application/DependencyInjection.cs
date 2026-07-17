using Dispatcher.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dispatcher.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDispatcherApplication(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        services.AddScoped<ICurrentUser, AnonymousCurrentUser>();

        return services;
    }
}
