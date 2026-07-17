namespace Dispatcher.Application.Devices.Commands;

public sealed record CreateModbusDeviceCommand(
    string Name,
    string Host,
    int Port,
    int PollIntervalMs,
    int TimeoutMs,
    int RetryCount,
    string? Description);
