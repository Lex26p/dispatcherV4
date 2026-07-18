namespace Dispatcher.Contracts.Telemetry;

public sealed record TelemetrySourceDto(
    Guid Id,
    string Code,
    string Name,
    string Protocol,
    string Endpoint,
    int ConfigurationSchemaVersion,
    string ConfigurationJson,
    bool HasSecretReference,
    string? MaskedSecretReference,
    string? Description,
    bool IsEnabled,
    bool IsArchived,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
