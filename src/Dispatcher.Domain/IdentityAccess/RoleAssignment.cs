using Dispatcher.Domain.Common;

namespace Dispatcher.Domain.IdentityAccess;

public sealed class RoleAssignment
{
    private RoleAssignment()
    {
    }

    private RoleAssignment(EntityId id, EntityId userId, EntityId roleId, EntityId? scopeId, string source, string? reason, DateTimeOffset createdAtUtc)
    {
        Id = id;
        UserId = userId;
        RoleId = roleId;
        ScopeId = scopeId;
        Source = NormalizeRequired(source, nameof(source));
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public EntityId Id { get; private set; }

    public EntityId UserId { get; private set; }

    public EntityId RoleId { get; private set; }

    public EntityId? ScopeId { get; private set; }

    public string Source { get; private set; } = string.Empty;

    public string? Reason { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? RevokedReason { get; private set; }

    public bool IsActive => RevokedAtUtc is null;

    public static RoleAssignment Grant(EntityId id, EntityId userId, EntityId roleId, EntityId? scopeId, string source, string? reason, DateTimeOffset createdAtUtc)
        => new(id, userId, roleId, scopeId, source, reason, createdAtUtc);

    public void Revoke(DateTimeOffset revokedAtUtc, string reason)
    {
        if (RevokedAtUtc is not null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Revocation reason is required.", nameof(reason));
        }

        RevokedAtUtc = revokedAtUtc.ToUniversalTime();
        RevokedReason = reason.Trim();
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
