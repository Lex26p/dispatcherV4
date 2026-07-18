namespace Dispatcher.Contracts.Assets;

public sealed record LocationDto(
    Guid Id,
    Guid? ParentLocationId,
    string Code,
    string Name,
    string? Description,
    bool IsArchived,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
