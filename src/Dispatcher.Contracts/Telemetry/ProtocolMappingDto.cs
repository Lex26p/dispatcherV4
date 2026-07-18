namespace Dispatcher.Contracts.Telemetry;

public sealed record ProtocolMappingDto(
    Guid Id,
    Guid DataPointId,
    Guid TelemetrySourceId,
    string Protocol,
    int MappingSchemaVersion,
    string MappingJson,
    bool IsArchived,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
