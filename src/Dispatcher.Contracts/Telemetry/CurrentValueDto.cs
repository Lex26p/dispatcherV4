namespace Dispatcher.Contracts.Telemetry;

public sealed record CurrentValueDto(
    Guid DataPointId,
    Guid? TelemetrySourceId,
    long Sequence,
    string ValueKind,
    string RawValue,
    string? Unit,
    string Quality,
    string Freshness,
    bool IsFresh,
    int FreshnessTimeoutSeconds,
    DateTimeOffset SourceTimestampUtc,
    DateTimeOffset ReceivedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string? ErrorMessage);
