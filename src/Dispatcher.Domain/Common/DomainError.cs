namespace Dispatcher.Domain.Common;

/// <summary>
/// Machine-readable domain error that can be translated to public API problem details by the application/API layer.
/// </summary>
public sealed record DomainError(string Code, string Message, string? Target = null)
{
    public static DomainError Create(string code, string message, string? target = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Error code is required.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Error message is required.", nameof(message));
        }

        return new DomainError(code.Trim(), message.Trim(), string.IsNullOrWhiteSpace(target) ? null : target.Trim());
    }
}
