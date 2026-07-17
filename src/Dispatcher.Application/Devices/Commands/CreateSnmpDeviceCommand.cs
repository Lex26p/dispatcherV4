using Dispatcher.Domain.Tags;

namespace Dispatcher.Application.Devices.Commands;

public sealed record CreateSnmpDeviceCommand(
    string Name,
    string Host,
    string Community,
    SnmpVersion Version,
    int Port,
    int PollIntervalMs,
    int TimeoutMs,
    int RetryCount,
    string? Description);
