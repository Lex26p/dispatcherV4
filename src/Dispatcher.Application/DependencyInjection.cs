using Dispatcher.Application.Devices;
using Dispatcher.Application.Tags;
using Microsoft.Extensions.DependencyInjection;

namespace Dispatcher.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ITagValueService, TagValueService>();

        return services;
    }
}
