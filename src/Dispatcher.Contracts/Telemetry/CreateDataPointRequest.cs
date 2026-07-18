namespace Dispatcher.Contracts.Telemetry;

public sealed record CreateDataPointRequest(
    Guid EquipmentId,
    string Code,
    string Name,
    string ValueKind,
    string? Unit,
    int FreshnessTimeoutSeconds,
    string? Description);
