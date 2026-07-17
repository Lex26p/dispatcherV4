using Dispatcher.Application.Devices.Commands;

namespace Dispatcher.Api.Contracts.Devices;

public sealed class CreateModbusDeviceRequest
{
    public string Name { get; init; } = string.Empty;

    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 502;

    public int PollIntervalMs { get; init; } = 1000;

    public int TimeoutMs { get; init; } = 3000;

    public int RetryCount { get; init; } = 3;

    public string? Description { get; init; }

    public CreateModbusDeviceCommand ToCommand()
    {
        return new CreateModbusDeviceCommand(
            Name,
            Host,
            Port,
            PollIntervalMs,
            TimeoutMs,
            RetryCount,
            Description);
    }
}
