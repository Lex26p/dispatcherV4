namespace Dispatcher.Contracts.Telemetry;

public sealed record HistoricalValueDto(
    Guid Id,
    Guid DataPointId,
    Guid? TelemetrySourceId,
    long Sequence,
    string ValueKind,
    string RawValue,
    string? Unit,
    string Quality,
    DateTimeOffset SourceTimestampUtc,
    DateTimeOffset ReceivedAtUtc,
    DateTimeOffset CreatedAtUtc,
    string? ErrorMessage);
