using Dispatcher.Domain.Common;

namespace Dispatcher.Domain.Telemetry;

/// <summary>
/// Technical source of telemetry samples. It is not Equipment and never stores secret plaintext.
/// </summary>
public sealed class TelemetrySource
{
    private TelemetrySource()
    {
    }

    private TelemetrySource(
        EntityId id,
        string code,
        string name,
        TelemetryProtocol protocol,
        string endpoint,
        int configurationSchemaVersion,
        string configurationJson,
        string? secretReference,
        string? description,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        Code = NormalizeCode(code);
        Name = NormalizeRequired(name, nameof(name), 200);
        Protocol = protocol;
        Endpoint = NormalizeRequired(endpoint, nameof(endpoint), 500);
        ConfigurationSchemaVersion = ValidateSchemaVersion(configurationSchemaVersion);
        ConfigurationJson = NormalizeJson(configurationJson);
        SecretReference = NormalizeSecretReference(secretReference);
        Description = NormalizeOptional(description, 1000);
        IsEnabled = false;
        IsArchived = false;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        UpdatedAtUtc = CreatedAtUtc;
    }

    public EntityId Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public TelemetryProtocol Protocol { get; private set; }

    public string Endpoint { get; private set; } = string.Empty;

    public int ConfigurationSchemaVersion { get; private set; }

    public string ConfigurationJson { get; private set; } = "{}";

    public string? SecretReference { get; private set; }

    public string? Description { get; private set; }

    public bool IsEnabled { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static TelemetrySource Create(
        EntityId id,
        string code,
        string name,
        TelemetryProtocol protocol,
        string endpoint,
        int configurationSchemaVersion,
        string configurationJson,
        string? secretReference,
        string? description,
        DateTimeOffset createdAtUtc)
    {
        return new TelemetrySource(id, code, name, protocol, endpoint, configurationSchemaVersion, configurationJson, secretReference, description, createdAtUtc);
    }

    public void Rename(string name, string? description, DateTimeOffset updatedAtUtc)
    {
        Name = NormalizeRequired(name, nameof(name), 200);
        Description = NormalizeOptional(description, 1000);
        Touch(updatedAtUtc);
    }

    public void ChangeConfiguration(string endpoint, int schemaVersion, string configurationJson, string? secretReference, DateTimeOffset updatedAtUtc)
    {
        Endpoint = NormalizeRequired(endpoint, nameof(endpoint), 500);
        ConfigurationSchemaVersion = ValidateSchemaVersion(schemaVersion);
        ConfigurationJson = NormalizeJson(configurationJson);
        SecretReference = NormalizeSecretReference(secretReference);
        Touch(updatedAtUtc);
    }

    public void Enable(DateTimeOffset updatedAtUtc)
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("Archived telemetry source cannot be enabled.");
        }

        IsEnabled = true;
        Touch(updatedAtUtc);
    }

    public void Disable(DateTimeOffset updatedAtUtc)
    {
        IsEnabled = false;
        Touch(updatedAtUtc);
    }

    public void Archive(DateTimeOffset updatedAtUtc)
    {
        IsEnabled = false;
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
            throw new ArgumentOutOfRangeException(nameof(version), "Configuration schema version must be positive.");
        }

        return version;
    }

    private static string NormalizeJson(string value)
    {
        var normalized = NormalizeRequired(value, nameof(value), 8000);
        if (!normalized.StartsWith('{') || !normalized.EndsWith('}'))
        {
            throw new ArgumentException("Telemetry source configuration must be a JSON object string.", nameof(value));
        }

        return normalized;
    }

    private static string? NormalizeSecretReference(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (!normalized.StartsWith("secret://", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Secret reference must use the secret:// scheme. Secret plaintext is forbidden.", nameof(value));
        }

        if (normalized.Length > 500)
        {
            throw new ArgumentException("Secret reference cannot exceed 500 characters.", nameof(value));
        }

        return normalized;
    }

    private static string NormalizeCode(string value)
    {
        var normalized = NormalizeRequired(value, nameof(value), 100).ToUpperInvariant();

        if (normalized.Any(character => !char.IsLetterOrDigit(character) && character is not '-' and not '_' and not '.'))
        {
            throw new ArgumentException("TelemetrySource code can contain only letters, digits, '-', '_' or '.'.", nameof(value));
        }

        return normalized;
    }

    private static string NormalizeRequired(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameterName);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", nameof(value));
        }

        return normalized;
    }
}
