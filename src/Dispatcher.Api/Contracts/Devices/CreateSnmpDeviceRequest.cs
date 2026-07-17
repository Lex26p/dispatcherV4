using Dispatcher.Application.Devices.Commands;
using Dispatcher.Domain.Tags;

namespace Dispatcher.Api.Contracts.Devices;

public sealed class CreateSnmpDeviceRequest
{
    public string Name { get; init; } = string.Empty;

    public string Host { get; init; } = string.Empty;

    public string Community { get; init; } = "public";

    public SnmpVersion Version { get; init; } = SnmpVersion.V2C;

    public int Port { get; init; } = 161;

    public int PollIntervalMs { get; init; } = 5000;

    public int TimeoutMs { get; init; } = 3000;

    public int RetryCount { get; init; } = 3;

    public string? Description { get; init; }

    public CreateSnmpDeviceCommand ToCommand()
    {
        return new CreateSnmpDeviceCommand(
            Name,
            Host,
            Community,
            Version,
            Port,
            PollIntervalMs,
            TimeoutMs,
            RetryCount,
            Description);
    }
}
