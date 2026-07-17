using Dispatcher.Domain.Common;
using Dispatcher.Domain.Tags;

namespace Dispatcher.Domain.Devices;

public sealed class DeviceConnectionSettings
{
    public string Host { get; private set; } = string.Empty;

    public int Port { get; private set; }

    public int PollIntervalMs { get; private set; }

    public int TimeoutMs { get; private set; }

    public int RetryCount { get; private set; }

    public SnmpVersion? SnmpVersion { get; private set; }

    public string? SnmpCommunity { get; private set; }

    private DeviceConnectionSettings()
    {
    }

    public static DeviceConnectionSettings CreateModbusTcp(
        string host,
        int port = 502,
        int pollIntervalMs = 1000,
        int timeoutMs = 3000,
        int retryCount = 3)
    {
        var settings = new DeviceConnectionSettings
        {
            Host = host,
            Port = port,
            PollIntervalMs = pollIntervalMs,
            TimeoutMs = timeoutMs,
            RetryCount = retryCount
        };

        settings.ValidateCommon();
        return settings;
    }

    public static DeviceConnectionSettings CreateSnmp(
        string host,
        string community,
        SnmpVersion version = Tags.SnmpVersion.V2C,
        int port = 161,
        int pollIntervalMs = 5000,
        int timeoutMs = 3000,
        int retryCount = 3)
    {
        var settings = new DeviceConnectionSettings
        {
            Host = host,
            Port = port,
            PollIntervalMs = pollIntervalMs,
            TimeoutMs = timeoutMs,
            RetryCount = retryCount,
            SnmpVersion = version,
            SnmpCommunity = community
        };

        settings.ValidateCommon();

        if (string.IsNullOrWhiteSpace(settings.SnmpCommunity))
        {
            throw new DomainException("SNMP community is required.");
        }

        return settings;
    }

    private void ValidateCommon()
    {
        if (string.IsNullOrWhiteSpace(Host))
        {
            throw new DomainException("Device host is required.");
        }

        if (Port is <= 0 or > 65535)
        {
            throw new DomainException("Device port must be between 1 and 65535.");
        }

        if (PollIntervalMs < 100)
        {
            throw new DomainException("Device poll interval must be at least 100 ms.");
        }

        if (TimeoutMs < 100)
        {
            throw new DomainException("Device timeout must be at least 100 ms.");
        }

        if (RetryCount < 0)
        {
            throw new DomainException("Device retry count cannot be negative.");
        }
    }
}
