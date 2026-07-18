namespace Dispatcher.Contracts.Identity;

public sealed record PermissionScopeDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsArchived,
    DateTimeOffset CreatedAtUtc);
