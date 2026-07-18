namespace Dispatcher.Contracts.Assets;

public sealed record EquipmentDto(
    Guid Id,
    Guid LocationId,
    string Code,
    string Name,
    string? Description,
    bool IsArchived,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
