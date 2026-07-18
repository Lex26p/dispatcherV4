namespace Dispatcher.Contracts.Identity;

public sealed record RoleDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    IReadOnlyList<string> Permissions,
    bool IsSystem,
    bool IsArchived);
