namespace Dispatcher.Contracts.Assets;

public sealed record UpdateEquipmentRequest(
    string Name,
    string? Description);
