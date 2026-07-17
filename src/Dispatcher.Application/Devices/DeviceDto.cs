using Dispatcher.Domain.Devices;
using Dispatcher.Domain.Tags;

namespace Dispatcher.Application.Devices;

public sealed record DeviceDto(
    Guid Id,
    string Name,
    DeviceProtocol Protocol,
    string Host,
    int Port,
    int PollIntervalMs,
    int TimeoutMs,
    int RetryCount,
    DeviceStatus Status,
    bool IsEnabled,
    string? Description,
    SnmpVersion? SnmpVersion,
    string? SnmpCommunity);
