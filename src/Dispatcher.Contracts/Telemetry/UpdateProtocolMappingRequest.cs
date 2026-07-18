namespace Dispatcher.Contracts.Telemetry;

public sealed record UpdateProtocolMappingRequest(
    int MappingSchemaVersion,
    string MappingJson);
