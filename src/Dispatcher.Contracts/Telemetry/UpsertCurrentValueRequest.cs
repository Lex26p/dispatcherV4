namespace Dispatcher.Contracts.Telemetry;

public sealed record UpsertCurrentValueRequest(
    Guid DataPointId,
    Guid? TelemetrySourceId,
    long Sequence,
    string ValueKind,
    string RawValue,
    string? Unit,
    string Quality,
    DateTimeOffset? SourceTimestampUtc,
    string? ErrorMessage);
