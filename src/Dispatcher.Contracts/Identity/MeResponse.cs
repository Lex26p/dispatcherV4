namespace Dispatcher.Contracts.Identity;

public sealed record MeResponse(
    bool IsAuthenticated,
    string? UserId,
    string? DisplayName,
    IReadOnlyList<string> Permissions);
