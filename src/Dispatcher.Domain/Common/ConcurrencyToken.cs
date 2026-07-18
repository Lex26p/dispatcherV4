namespace Dispatcher.Domain.Common;

/// <summary>
/// Opaque optimistic-concurrency token for aggregate updates and HTTP ETag mapping.
/// </summary>
public readonly record struct ConcurrencyToken
{
    public string Value { get; }

    private ConcurrencyToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Concurrency token is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public static ConcurrencyToken New() => new(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));

    public static ConcurrencyToken FromString(string value) => new(value);

    public override string ToString() => Value;
}
