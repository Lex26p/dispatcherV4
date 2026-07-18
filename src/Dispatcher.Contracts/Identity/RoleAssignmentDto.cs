namespace Dispatcher.Contracts.Identity;

public sealed record RoleAssignmentDto(
    Guid Id,
    Guid UserId,
    string UserDisplayName,
    Guid RoleId,
    string RoleCode,
    string RoleName,
    Guid? ScopeId,
    string? ScopeCode,
    string Source,
    string? Reason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? RevokedAtUtc,
    string? RevokedReason,
    bool IsActive);
