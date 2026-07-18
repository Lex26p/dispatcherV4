using Dispatcher.Domain.Common;

namespace Dispatcher.Domain.IdentityAccess;

public sealed class UserAccount
{
    private UserAccount()
    {
    }

    private UserAccount(EntityId id, string externalId, string displayName, string email, DateTimeOffset createdAtUtc)
    {
        Id = id;
        ExternalId = NormalizeRequired(externalId, nameof(externalId));
        DisplayName = NormalizeRequired(displayName, nameof(displayName));
        Email = NormalizeRequired(email, nameof(email)).ToLowerInvariant();
        IsActive = true;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public EntityId Id { get; private set; }

    public string ExternalId { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static UserAccount Create(EntityId id, string externalId, string displayName, string email, DateTimeOffset createdAtUtc)
        => new(id, externalId, displayName, email, createdAtUtc);

    public void Rename(string displayName)
    {
        DisplayName = NormalizeRequired(displayName, nameof(displayName));
    }

    public void ChangeEmail(string email)
    {
        Email = NormalizeRequired(email, nameof(email)).ToLowerInvariant();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }
}
