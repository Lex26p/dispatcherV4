using Dispatcher.Domain.Telemetry;

namespace Dispatcher.UnitTests.Domain;

public sealed class TypedValueTests
{
    [Fact]
    public void Decimal_value_uses_invariant_raw_representation_and_unit()
    {
        var value = TypedValue.FromDecimal(12.34m, " °C ");

        Assert.Equal(TypedValueKind.Decimal, value.Kind);
        Assert.Equal("12.34", value.RawValue);
        Assert.Equal("°C", value.Unit);
        Assert.Equal(12.34m, value.AsDecimal());
    }

    [Fact]
    public void Boolean_value_roundtrips()
    {
        var value = TypedValue.FromBoolean(true);

        Assert.True(value.AsBoolean());
        Assert.Equal("true", value.RawValue);
    }

    [Fact]
    public void Wrong_accessor_throws()
    {
        var value = TypedValue.FromText("online");

        Assert.Throws<InvalidOperationException>(() => value.AsDecimal());
    }
}
