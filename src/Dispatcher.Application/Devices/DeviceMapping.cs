using Dispatcher.Domain.Devices;

namespace Dispatcher.Application.Devices;

public static class DeviceMapping
{
    public static DeviceDto ToDto(this Device device)
    {
        return new DeviceDto(
            device.Id,
            device.Name,
            device.Protocol,
            device.ConnectionSettings.Host,
            device.ConnectionSettings.Port,
            device.ConnectionSettings.PollIntervalMs,
            device.ConnectionSettings.TimeoutMs,
            device.ConnectionSettings.RetryCount,
            device.Status,
            device.IsEnabled,
            device.Description,
            device.ConnectionSettings.SnmpVersion,
            device.ConnectionSettings.SnmpCommunity);
    }
}
