using System.Globalization;

namespace Dispatcher.Domain.Telemetry;

public enum TypedValueKind
{
    Boolean = 1,
    Integer = 2,
    Decimal = 3,
    Text = 4,
}

/// <summary>
/// Protocol-neutral scalar value. Unit is optional and belongs to the value/DataPoint contract, not to a protocol register/OID.
/// </summary>
public sealed record TypedValue
{
    private TypedValue(TypedValueKind kind, string rawValue, string? unit)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            throw new ArgumentException("Typed value cannot be empty.", nameof(rawValue));
        }

        Kind = kind;
        RawValue = rawValue.Trim();
        Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
    }

    public TypedValueKind Kind { get; }

    public string RawValue { get; }

    public string? Unit { get; }

    public static TypedValue FromBoolean(bool value, string? unit = null) => new(TypedValueKind.Boolean, value ? "true" : "false", unit);

    public static TypedValue FromInteger(long value, string? unit = null) => new(TypedValueKind.Integer, value.ToString(CultureInfo.InvariantCulture), unit);

    public static TypedValue FromDecimal(decimal value, string? unit = null) => new(TypedValueKind.Decimal, value.ToString(CultureInfo.InvariantCulture), unit);

    public static TypedValue FromText(string value, string? unit = null) => new(TypedValueKind.Text, value, unit);

    public bool AsBoolean() => Kind == TypedValueKind.Boolean
        ? bool.Parse(RawValue)
        : throw new InvalidOperationException("Typed value is not a boolean.");

    public long AsInteger() => Kind == TypedValueKind.Integer
        ? long.Parse(RawValue, CultureInfo.InvariantCulture)
        : throw new InvalidOperationException("Typed value is not an integer.");

    public decimal AsDecimal() => Kind == TypedValueKind.Decimal
        ? decimal.Parse(RawValue, CultureInfo.InvariantCulture)
        : throw new InvalidOperationException("Typed value is not a decimal.");

    public string AsText() => Kind == TypedValueKind.Text
        ? RawValue
        : throw new InvalidOperationException("Typed value is not text.");
}
