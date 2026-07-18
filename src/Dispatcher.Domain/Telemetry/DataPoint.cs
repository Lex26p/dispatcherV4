using Dispatcher.Domain.Common;

namespace Dispatcher.Domain.Telemetry;

/// <summary>
/// Protocol-neutral product signal/parameter. Register addresses, OIDs and driver-specific fields live in ProtocolMapping.
/// </summary>
public sealed class DataPoint
{
    private DataPoint()
    {
    }

    private DataPoint(
        EntityId id,
        EntityId equipmentId,
        string code,
        string name,
        TypedValueKind valueKind,
        string? unit,
        int freshnessTimeoutSeconds,
        string? description,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        EquipmentId = equipmentId;
        Code = NormalizeCode(code);
        Name = NormalizeRequired(name, nameof(name), 200);
        ValueKind = valueKind;
        Unit = NormalizeOptional(unit, 40);
        FreshnessTimeoutSeconds = ValidateFreshnessTimeout(freshnessTimeoutSeconds);
        Description = NormalizeOptional(description, 1000);
        IsArchived = false;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        UpdatedAtUtc = CreatedAtUtc;
    }

    public EntityId Id { get; private set; }

    public EntityId EquipmentId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public TypedValueKind ValueKind { get; private set; }

    public string? Unit { get; private set; }

    public int FreshnessTimeoutSeconds { get; private set; }

    public string? Description { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static DataPoint Create(
        EntityId id,
        EntityId equipmentId,
        string code,
        string name,
        TypedValueKind valueKind,
        string? unit,
        int freshnessTimeoutSeconds,
        string? description,
        DateTimeOffset createdAtUtc)
    {
        return new DataPoint(id, equipmentId, code, name, valueKind, unit, freshnessTimeoutSeconds, description, createdAtUtc);
    }

    public void Rename(string name, string? description, DateTimeOffset updatedAtUtc)
    {
        Name = NormalizeRequired(name, nameof(name), 200);
        Description = NormalizeOptional(description, 1000);
        Touch(updatedAtUtc);
    }

    public void ChangeCode(string code, DateTimeOffset updatedAtUtc)
    {
        Code = NormalizeCode(code);
        Touch(updatedAtUtc);
    }

    public void ChangeUnit(string? unit, DateTimeOffset updatedAtUtc)
    {
        Unit = NormalizeOptional(unit, 40);
        Touch(updatedAtUtc);
    }

    public void ChangeFreshnessPolicy(int freshnessTimeoutSeconds, DateTimeOffset updatedAtUtc)
    {
        FreshnessTimeoutSeconds = ValidateFreshnessTimeout(freshnessTimeoutSeconds);
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

    private static int ValidateFreshnessTimeout(int seconds)
    {
        if (seconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), "Freshness timeout must be positive.");
        }

        if (seconds > 86_400)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), "Freshness timeout cannot exceed one day in the baseline model.");
        }

        return seconds;
    }

    private static string NormalizeCode(string value)
    {
        var normalized = NormalizeRequired(value, nameof(value), 100).ToUpperInvariant();

        if (normalized.Any(character => !char.IsLetterOrDigit(character) && character is not '-' and not '_' and not '.'))
        {
            throw new ArgumentException("DataPoint code can contain only letters, digits, '-', '_' or '.'.", nameof(value));
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
