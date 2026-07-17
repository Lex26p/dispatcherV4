using Dispatcher.Domain.Common;
using Dispatcher.Domain.Tags;

namespace Dispatcher.Domain.Devices;

public sealed class Device : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;

    public DeviceProtocol Protocol { get; private set; }

    public DeviceConnectionSettings ConnectionSettings { get; private set; } = null!;

    public DeviceStatus Status { get; private set; } = DeviceStatus.Unknown;

    public bool IsEnabled { get; private set; }

    public string? Description { get; private set; }

    private Device()
    {
    }

    public static Device CreateModbusTcp(
        string name,
        string host,
        int port = 502,
        int pollIntervalMs = 1000,
        int timeoutMs = 3000,
        int retryCount = 3,
        string? description = null)
    {
        var device = new Device
        {
            Name = name,
            Protocol = DeviceProtocol.ModbusTcp,
            ConnectionSettings = DeviceConnectionSettings.CreateModbusTcp(
                host,
                port,
                pollIntervalMs,
                timeoutMs,
                retryCount),
            Description = description,
            IsEnabled = true,
            Status = DeviceStatus.Unknown
        };

        device.ValidateName();
        return device;
    }

    public static Device CreateSnmp(
        string name,
        string host,
        string community,
        SnmpVersion version = SnmpVersion.V2C,
        int port = 161,
        int pollIntervalMs = 5000,
        int timeoutMs = 3000,
        int retryCount = 3,
        string? description = null)
    {
        var device = new Device
        {
            Name = name,
            Protocol = DeviceProtocol.Snmp,
            ConnectionSettings = DeviceConnectionSettings.CreateSnmp(
                host,
                community,
                version,
                port,
                pollIntervalMs,
                timeoutMs,
                retryCount),
            Description = description,
            IsEnabled = true,
            Status = DeviceStatus.Unknown
        };

        device.ValidateName();
        return device;
    }

    public void Rename(string name)
    {
        Name = name;
        ValidateName();
        MarkUpdated();
    }

    public void ChangeDescription(string? description)
    {
        Description = description;
        MarkUpdated();
    }

    public void ChangeConnectionSettings(DeviceConnectionSettings connectionSettings)
    {
        ConnectionSettings = connectionSettings ?? throw new DomainException("Device connection settings are required.");
        MarkUpdated();
    }

    public void Enable()
    {
        IsEnabled = true;
        Status = DeviceStatus.Unknown;
        MarkUpdated();
    }

    public void Disable()
    {
        IsEnabled = false;
        Status = DeviceStatus.Disabled;
        MarkUpdated();
    }

    public void SetStatus(DeviceStatus status)
    {
        if (!IsEnabled && status != DeviceStatus.Disabled)
        {
            throw new DomainException("Disabled device can only have Disabled status.");
        }

        Status = status;
        MarkUpdated();
    }

    private void ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new DomainException("Device name is required.");
        }
    }
}
