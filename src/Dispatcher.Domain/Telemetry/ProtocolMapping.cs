using Dispatcher.Domain.Common;

namespace Dispatcher.Domain.Telemetry;

/// <summary>
/// Driver-specific mapping from a protocol-neutral DataPoint to a TelemetrySource.
/// Mapping details stay outside Equipment and DataPoint identity.
/// </summary>
public sealed class ProtocolMapping
{
    private ProtocolMapping()
    {
    }

    private ProtocolMapping(
        EntityId id,
        EntityId dataPointId,
        EntityId telemetrySourceId,
        TelemetryProtocol protocol,
        int mappingSchemaVersion,
        string mappingJson,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        DataPointId = dataPointId;
        TelemetrySourceId = telemetrySourceId;
        Protocol = protocol;
        MappingSchemaVersion = ValidateSchemaVersion(mappingSchemaVersion);
        MappingJson = NormalizeJson(mappingJson);
        IsArchived = false;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        UpdatedAtUtc = CreatedAtUtc;
    }

    public EntityId Id { get; private set; }

    public EntityId DataPointId { get; private set; }

    public EntityId TelemetrySourceId { get; private set; }

    public TelemetryProtocol Protocol { get; private set; }

    public int MappingSchemaVersion { get; private set; }

    public string MappingJson { get; private set; } = "{}";

    public bool IsArchived { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static ProtocolMapping Create(
        EntityId id,
        EntityId dataPointId,
        EntityId telemetrySourceId,
        TelemetryProtocol protocol,
        int mappingSchemaVersion,
        string mappingJson,
        DateTimeOffset createdAtUtc)
    {
        return new ProtocolMapping(id, dataPointId, telemetrySourceId, protocol, mappingSchemaVersion, mappingJson, createdAtUtc);
    }

    public void ChangeMapping(int mappingSchemaVersion, string mappingJson, DateTimeOffset updatedAtUtc)
    {
        MappingSchemaVersion = ValidateSchemaVersion(mappingSchemaVersion);
        MappingJson = NormalizeJson(mappingJson);
        Touch(updatedAtUtc);
    }

    public void Archive(DateTimeOffset updatedAtUtc)
    {
        IsArchived = true;
        Touch(updatedAtUtc);
    }

    public void Restore(DateTimeOffset updatedAtUtc)
    {
        IsArchived = false;
        Touch(updatedAtUtc);
    }

    private void Touch(DateTimeOffset updatedAtUtc)
    {
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    private static int ValidateSchemaVersion(int version)
    {
        if (version <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version), "Mapping schema version must be positive.");
        }

        return version;
    }

    private static string NormalizeJson(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Mapping JSON is required.", nameof(value));
        }

        var normalized = value.Trim();
        if (!normalized.StartsWith('{') || !normalized.EndsWith('}'))
        {
            throw new ArgumentException("Protocol mapping must be a JSON object string.", nameof(value));
        }

        if (normalized.Length > 8000)
        {
            throw new ArgumentException("Protocol mapping cannot exceed 8000 characters.", nameof(value));
        }

        return normalized;
    }
}
