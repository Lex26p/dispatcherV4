using Dispatcher.Domain.Common;

namespace Dispatcher.UnitTests.Domain;

public sealed class UtcTimestampTests
{
    [Fact]
    public void From_DateTimeOffset_converts_non_utc_offset_to_utc()
    {
        var timestamp = UtcTimestamp.From(new DateTimeOffset(2026, 7, 18, 12, 0, 0, TimeSpan.FromHours(2)));

        Assert.Equal(TimeSpan.Zero, timestamp.Value.Offset);
        Assert.Equal(10, timestamp.Value.Hour);
    }

    [Fact]
    public void From_unspecified_DateTime_treats_value_as_utc()
    {
        var timestamp = UtcTimestamp.From(new DateTime(2026, 7, 18, 12, 0, 0, DateTimeKind.Unspecified));

        Assert.Equal(TimeSpan.Zero, timestamp.Value.Offset);
        Assert.Equal(12, timestamp.Value.Hour);
    }

    [Fact]
    public void Parse_rejects_invalid_timestamp()
    {
        Assert.Throws<ArgumentException>(() => UtcTimestamp.Parse("not-a-date"));
    }
}
