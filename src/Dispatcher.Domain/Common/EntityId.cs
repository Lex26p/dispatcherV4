namespace Dispatcher.Domain.Common;

/// <summary>
/// Stable identifier for domain entities exposed as a non-empty GUID.
/// </summary>
public readonly record struct EntityId
{
    public Guid Value { get; }

    private EntityId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Entity id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static EntityId New() => new(Guid.NewGuid());

    public static EntityId From(Guid value) => new(value);

    public static EntityId FromString(string value)
    {
        if (!Guid.TryParse(value, out var guid))
        {
            throw new ArgumentException("Entity id must be a valid GUID.", nameof(value));
        }

        return From(guid);
    }

    public override string ToString() => Value.ToString("D");
}
