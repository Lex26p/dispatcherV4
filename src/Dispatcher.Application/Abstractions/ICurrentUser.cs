namespace Dispatcher.Application.Abstractions;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? UserId { get; }

    string? DisplayName { get; }

    IReadOnlySet<string> Permissions { get; }
}
