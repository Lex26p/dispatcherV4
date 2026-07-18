namespace Dispatcher.Contracts.Telemetry;

public sealed record DataPointDto(
    Guid Id,
    Guid EquipmentId,
    string Code,
    string Name,
    string ValueKind,
    string? Unit,
    int FreshnessTimeoutSeconds,
    string? Description,
    bool IsArchived,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
