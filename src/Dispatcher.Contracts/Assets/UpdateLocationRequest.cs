namespace Dispatcher.Contracts.Assets;

public sealed record UpdateLocationRequest(
    string Name,
    string? Description);
