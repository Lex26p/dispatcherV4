using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;

namespace Dispatcher.UnitTests.Telemetry;

public sealed class DataPointTests
{
    [Fact]
    public void Create_normalizes_code_and_keeps_protocol_out_of_identity()
    {
        var point = DataPoint.Create(
            EntityId.New(),
            EntityId.New(),
            "  temp-001  ",
            "  Temperature 001  ",
            TypedValueKind.Decimal,
            " °C ",
            freshnessTimeoutSeconds: 30,
            "  Supply temperature  ",
            DateTimeOffset.UtcNow);

        Assert.Equal("TEMP-001", point.Code);
        Assert.Equal("Temperature 001", point.Name);
        Assert.Equal(TypedValueKind.Decimal, point.ValueKind);
        Assert.Equal("°C", point.Unit);
        Assert.Equal(30, point.FreshnessTimeoutSeconds);
        Assert.Equal("Supply temperature", point.Description);
        Assert.False(point.IsArchived);
    }

    [Fact]
    public void Create_rejects_invalid_freshness_timeout()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DataPoint.Create(
            EntityId.New(),
            EntityId.New(),
            "TEMP",
            "Temperature",
            TypedValueKind.Decimal,
            "°C",
            freshnessTimeoutSeconds: 0,
            null,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Archive_marks_point_archived_without_losing_identity()
    {
        var id = EntityId.New();
        var point = DataPoint.Create(id, EntityId.New(), "TEMP", "Temperature", TypedValueKind.Decimal, "°C", 30, null, DateTimeOffset.UtcNow);

        point.Archive(DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.Equal(id, point.Id);
        Assert.True(point.IsArchived);
    }
}
