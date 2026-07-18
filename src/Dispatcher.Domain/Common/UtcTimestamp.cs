namespace Dispatcher.Domain.Common;

/// <summary>
/// UTC-only timestamp used by domain objects and public contracts.
/// </summary>
public readonly record struct UtcTimestamp
{
    public DateTimeOffset Value { get; }

    private UtcTimestamp(DateTimeOffset value)
    {
        Value = value.Offset == TimeSpan.Zero ? value : value.ToUniversalTime();
    }

    public static UtcTimestamp From(DateTimeOffset value) => new(value);

    public static UtcTimestamp From(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => new DateTimeOffset(value),
            DateTimeKind.Local => new DateTimeOffset(value).ToUniversalTime(),
            _ => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)),
        };

        return new UtcTimestamp(utc);
    }

    public static UtcTimestamp Parse(string value)
    {
        if (!DateTimeOffset.TryParse(value, out var parsed))
        {
            throw new ArgumentException("Timestamp must be a valid ISO-8601 date/time value.", nameof(value));
        }

        return From(parsed);
    }

    public override string ToString() => Value.ToString("O");
}
