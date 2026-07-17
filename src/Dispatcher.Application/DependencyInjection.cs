using Microsoft.Extensions.DependencyInjection;

namespace Dispatcher.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDispatcherApplication(this IServiceCollection services)
    {
        return services;
    }
}
