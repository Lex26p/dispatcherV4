using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;

namespace Dispatcher.UnitTests.Telemetry;

public sealed class CurrentValueTests
{
    [Fact]
    public void Create_keeps_required_operator_value_attributes()
    {
        var dataPointId = EntityId.New();
        var sourceId = EntityId.New();
        var sourceTimestamp = DateTimeOffset.UtcNow.AddSeconds(-2);
        var receivedAt = DateTimeOffset.UtcNow;

        var value = CurrentValue.Create(
            dataPointId,
            sourceId,
            sequence: 10,
            TypedValue.FromDecimal(12.5m, "C"),
            DataQuality.Good,
            sourceTimestamp,
            receivedAt);

        Assert.Equal(dataPointId, value.DataPointId);
        Assert.Equal(sourceId, value.TelemetrySourceId);
        Assert.Equal(10, value.Sequence);
        Assert.Equal(TypedValueKind.Decimal, value.ValueKind);
        Assert.Equal("12.5", value.RawValue);
        Assert.Equal("C", value.Unit);
        Assert.Equal(DataQuality.Good, value.Quality);
        Assert.Equal(sourceTimestamp, value.SourceTimestampUtc);
        Assert.Equal(receivedAt, value.ReceivedAtUtc);
        Assert.True(value.IsGood);
    }

    [Fact]
    public void TryApplySample_rejects_duplicate_or_older_sequence()
    {
        var value = CurrentValue.Create(
            EntityId.New(),
            EntityId.New(),
            sequence: 10,
            TypedValue.FromInteger(10, "A"),
            DataQuality.Good,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);

        var applied = value.TryApplySample(
            EntityId.New(),
            sequence: 9,
            TypedValue.FromInteger(99, "A"),
            DataQuality.Bad,
            DateTimeOffset.UtcNow.AddSeconds(1),
            DateTimeOffset.UtcNow.AddSeconds(1),
            "older sample");

        Assert.False(applied);
        Assert.Equal(10, value.Sequence);
        Assert.Equal("10", value.RawValue);
        Assert.Equal(DataQuality.Good, value.Quality);
    }

    [Fact]
    public void TryApplySample_accepts_newer_sequence()
    {
        var value = CurrentValue.Create(
            EntityId.New(),
            EntityId.New(),
            sequence: 10,
            TypedValue.FromInteger(10, "A"),
            DataQuality.Good,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);

        var applied = value.TryApplySample(
            EntityId.New(),
            sequence: 11,
            TypedValue.FromInteger(11, "A"),
            DataQuality.Uncertain,
            DateTimeOffset.UtcNow.AddSeconds(1),
            DateTimeOffset.UtcNow.AddSeconds(1),
            "diagnostic");

        Assert.True(applied);
        Assert.Equal(11, value.Sequence);
        Assert.Equal("11", value.RawValue);
        Assert.Equal(DataQuality.Uncertain, value.Quality);
        Assert.Equal("diagnostic", value.ErrorMessage);
    }
}
