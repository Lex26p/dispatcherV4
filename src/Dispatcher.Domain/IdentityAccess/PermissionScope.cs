using Dispatcher.Domain.Common;

namespace Dispatcher.Domain.IdentityAccess;

public sealed class PermissionScope
{
    private PermissionScope()
    {
    }

    private PermissionScope(EntityId id, string code, string name, string? description, DateTimeOffset createdAtUtc)
    {
        Id = id;
        Code = NormalizeCode(code);
        Name = NormalizeRequired(name, nameof(name));
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsArchived = false;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public EntityId Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static PermissionScope Create(EntityId id, string code, string name, string? description, DateTimeOffset createdAtUtc)
        => new(id, code, name, description, createdAtUtc);

    public void Archive() => IsArchived = true;

    private static string NormalizeCode(string code)
    {
        var normalized = NormalizeRequired(code, nameof(code)).ToLowerInvariant();

        if (normalized.Any(ch => !(char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_' or '.')))
        {
            throw new ArgumentException("Scope code may contain only ASCII letters, digits, dash, underscore and dot.", nameof(code));
        }

        return normalized;
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }
}
