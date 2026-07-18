using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;

namespace Dispatcher.UnitTests.Telemetry;

public sealed class HistoricalValueTests
{
    [Fact]
    public void Create_captures_append_only_sample_shape()
    {
        var id = EntityId.New();
        var dataPointId = EntityId.New();
        var sourceId = EntityId.New();
        var sourceTimestamp = DateTimeOffset.UtcNow.AddSeconds(-5);
        var receivedAt = DateTimeOffset.UtcNow;

        var sample = HistoricalValue.Create(
            id,
            dataPointId,
            sourceId,
            sequence: 42,
            TypedValue.FromText("online"),
            DataQuality.Good,
            sourceTimestamp,
            receivedAt);

        Assert.Equal(id, sample.Id);
        Assert.Equal(dataPointId, sample.DataPointId);
        Assert.Equal(sourceId, sample.TelemetrySourceId);
        Assert.Equal(42, sample.Sequence);
        Assert.Equal(TypedValueKind.Text, sample.ValueKind);
        Assert.Equal("online", sample.RawValue);
        Assert.Equal(DataQuality.Good, sample.Quality);
        Assert.Equal(sourceTimestamp, sample.SourceTimestampUtc);
        Assert.Equal(receivedAt, sample.ReceivedAtUtc);
        Assert.Equal(receivedAt, sample.CreatedAtUtc);
    }

    [Fact]
    public void Create_rejects_non_positive_sequence()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => HistoricalValue.Create(
            EntityId.New(),
            EntityId.New(),
            null,
            sequence: 0,
            TypedValue.FromBoolean(true),
            DataQuality.Good,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow));
    }
}
