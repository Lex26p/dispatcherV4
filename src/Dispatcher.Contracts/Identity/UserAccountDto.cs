namespace Dispatcher.Contracts.Identity;

public sealed record UserAccountDto(
    Guid Id,
    string ExternalId,
    string DisplayName,
    string Email,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);
