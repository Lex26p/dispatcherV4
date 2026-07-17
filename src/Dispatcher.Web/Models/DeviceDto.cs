namespace Dispatcher.Web.Models;

public sealed record DeviceDto(
    Guid Id,
    string Name,
    int Protocol,
    string Host,
    int Port,
    int PollIntervalMs,
    int TimeoutMs,
    int RetryCount,
    int Status,
    bool IsEnabled,
    string? Description,
    int? SnmpVersion,
    string? SnmpCommunity);
