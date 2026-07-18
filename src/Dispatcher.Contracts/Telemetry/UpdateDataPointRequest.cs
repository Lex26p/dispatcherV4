namespace Dispatcher.Contracts.Telemetry;

public sealed record UpdateDataPointRequest(
    string Name,
    string? Unit,
    int FreshnessTimeoutSeconds,
    string? Description);
