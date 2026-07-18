namespace Dispatcher.Contracts.Identity;

public sealed record GrantRoleRequest(
    Guid UserId,
    Guid RoleId,
    Guid? ScopeId,
    string? Reason);
