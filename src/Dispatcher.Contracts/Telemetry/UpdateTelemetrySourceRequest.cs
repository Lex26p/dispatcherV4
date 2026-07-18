namespace Dispatcher.Contracts.Telemetry;

public sealed record UpdateTelemetrySourceRequest(
    string Name,
    string Endpoint,
    int ConfigurationSchemaVersion,
    string ConfigurationJson,
    string? SecretReference,
    string? Description);
