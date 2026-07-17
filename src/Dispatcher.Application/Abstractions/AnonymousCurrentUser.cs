namespace Dispatcher.Application.Abstractions;

public sealed class AnonymousCurrentUser : ICurrentUser
{
    public bool IsAuthenticated => false;

    public string? UserId => null;

    public string? DisplayName => null;

    public IReadOnlySet<string> Permissions { get; } = new HashSet<string>();
}
