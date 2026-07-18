namespace Dispatcher.Contracts.Assets;

public sealed record CreateLocationRequest(
    Guid? ParentLocationId,
    string Code,
    string Name,
    string? Description);
