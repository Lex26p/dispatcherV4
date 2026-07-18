namespace Dispatcher.Contracts.Assets;

public sealed record CreateEquipmentRequest(
    Guid LocationId,
    string Code,
    string Name,
    string? Description);
