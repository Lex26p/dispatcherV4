using Dispatcher.Domain.Common;

namespace Dispatcher.Domain.IdentityAccess;

public sealed class Role
{
    private Role()
    {
    }

    private Role(EntityId id, string code, string name, string? description, IEnumerable<string> permissions, bool isSystem)
    {
        Id = id;
        Code = NormalizeCode(code);
        Name = NormalizeRequired(name, nameof(name));
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        PermissionsText = NormalizePermissions(permissions);
        IsSystem = isSystem;
        IsArchived = false;
    }

    public EntityId Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string PermissionsText { get; private set; } = string.Empty;

    public bool IsSystem { get; private set; }

    public bool IsArchived { get; private set; }

    public IReadOnlySet<string> PermissionSet => PermissionsText
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static Role Create(EntityId id, string code, string name, string? description, IEnumerable<string> permissions, bool isSystem = false)
        => new(id, code, name, description, permissions, isSystem);

    public bool HasPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        var set = PermissionSet;
        return set.Contains("*") || set.Contains(permission.Trim());
    }

    public void ReplacePermissions(IEnumerable<string> permissions)
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("Archived role cannot be changed.");
        }

        PermissionsText = NormalizePermissions(permissions);
    }

    public void Archive()
    {
        if (IsSystem)
        {
            throw new InvalidOperationException("System role cannot be archived.");
        }

        IsArchived = true;
    }

    private static string NormalizePermissions(IEnumerable<string> permissions)
    {
        var normalized = permissions
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Select(permission => permission.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(permission => permission, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new ArgumentException("Role requires at least one permission.", nameof(permissions));
        }

        return string.Join(';', normalized);
    }

    private static string NormalizeCode(string code)
    {
        var normalized = NormalizeRequired(code, nameof(code)).ToLowerInvariant();

        if (normalized.Any(ch => !(char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_' or '.')))
        {
            throw new ArgumentException("Role code may contain only ASCII letters, digits, dash, underscore and dot.", nameof(code));
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
