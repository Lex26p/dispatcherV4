namespace Dispatcher.Contracts.Telemetry;

public sealed record CreateProtocolMappingRequest(
    Guid DataPointId,
    Guid TelemetrySourceId,
    string Protocol,
    int MappingSchemaVersion,
    string MappingJson);
