namespace Dispatcher.Contracts.Telemetry;

public sealed record CreateTelemetrySourceRequest(
    string Code,
    string Name,
    string Protocol,
    string Endpoint,
    int ConfigurationSchemaVersion,
    string ConfigurationJson,
    string? SecretReference,
    string? Description);
