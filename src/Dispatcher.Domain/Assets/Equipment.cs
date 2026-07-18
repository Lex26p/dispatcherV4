using Dispatcher.Domain.Common;

namespace Dispatcher.Domain.Assets;

/// <summary>
/// Canonical user-facing asset/equipment record.
/// Protocol connection details, registers, OIDs and telemetry source settings do not live here.
/// </summary>
public sealed class Equipment
{
    private Equipment()
    {
    }

    private Equipment(
        EntityId id,
        EntityId locationId,
        string code,
        string name,
        string? description,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        LocationId = locationId;
        Code = NormalizeCode(code);
        Name = NormalizeRequired(name, nameof(name), 200);
        Description = NormalizeOptional(description, 1000);
        IsArchived = false;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        UpdatedAtUtc = CreatedAtUtc;
    }

    public EntityId Id { get; private set; }

    public EntityId LocationId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Equipment Create(
        EntityId id,
        EntityId locationId,
        string code,
        string name,
        string? description,
        DateTimeOffset createdAtUtc)
    {
        return new Equipment(id, locationId, code, name, description, createdAtUtc);
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

    public void MoveToLocation(EntityId locationId, DateTimeOffset updatedAtUtc)
    {
        LocationId = locationId;
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

    private static string NormalizeCode(string value)
    {
        var normalized = NormalizeRequired(value, nameof(value), 80).ToUpperInvariant();

        if (normalized.Any(character => !char.IsLetterOrDigit(character) && character is not '-' and not '_' and not '.'))
        {
            throw new ArgumentException("Equipment code can contain only letters, digits, '-', '_' or '.'.", nameof(value));
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
