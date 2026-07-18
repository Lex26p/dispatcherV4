using Dispatcher.Application.Abstractions;
using Dispatcher.Application.Telemetry.Values;
using Dispatcher.Contracts.Telemetry;
using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;

namespace Dispatcher.UnitTests.Telemetry;

public sealed class TelemetryValueServiceTests
{
    [Fact]
    public async Task Upsert_current_value_creates_current_and_history()
    {
        var dataPoint = DataPoint.Create(EntityId.New(), EntityId.New(), "TEMP.1", "Temperature", TypedValueKind.Decimal, "C", 60, null, FixedClock.Now);
        var repository = new InMemoryTelemetryValueRepository(dataPoint);
        var service = new TelemetryValueService(repository, new FixedClock());

        var response = await service.UpsertCurrentValueAsync(new UpsertCurrentValueRequest(
            dataPoint.Id.Value,
            null,
            1,
            "Decimal",
            "21.5",
            "C",
            "Good",
            FixedClock.Now,
            null), CancellationToken.None);

        Assert.True(response.Applied);
        Assert.Equal("created", response.Result);
        Assert.Equal("21.5", response.CurrentValue.RawValue);
        Assert.Single(repository.History);
    }

    [Fact]
    public async Task Upsert_current_value_ignores_older_sequence()
    {
        var dataPoint = DataPoint.Create(EntityId.New(), EntityId.New(), "TEMP.2", "Temperature", TypedValueKind.Decimal, "C", 60, null, FixedClock.Now);
        var repository = new InMemoryTelemetryValueRepository(dataPoint);
        var service = new TelemetryValueService(repository, new FixedClock());

        await service.UpsertCurrentValueAsync(new UpsertCurrentValueRequest(dataPoint.Id.Value, null, 10, "Decimal", "22.0", "C", "Good", FixedClock.Now, null), CancellationToken.None);
        var response = await service.UpsertCurrentValueAsync(new UpsertCurrentValueRequest(dataPoint.Id.Value, null, 9, "Decimal", "10.0", "C", "Good", FixedClock.Now, null), CancellationToken.None);

        Assert.False(response.Applied);
        Assert.Equal("ignored_out_of_order", response.Result);
        Assert.Equal("22.0", response.CurrentValue.RawValue);
        Assert.Single(repository.History);
    }

    [Fact]
    public async Task Upsert_current_value_rejects_wrong_value_kind()
    {
        var dataPoint = DataPoint.Create(EntityId.New(), EntityId.New(), "TEMP.3", "Temperature", TypedValueKind.Decimal, "C", 60, null, FixedClock.Now);
        var repository = new InMemoryTelemetryValueRepository(dataPoint);
        var service = new TelemetryValueService(repository, new FixedClock());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpsertCurrentValueAsync(new UpsertCurrentValueRequest(
            dataPoint.Id.Value,
            null,
            1,
            "Text",
            "bad",
            "C",
            "Good",
            FixedClock.Now,
            null), CancellationToken.None));
    }

    [Fact]
    public async Task History_query_requires_bounded_range()
    {
        var dataPoint = DataPoint.Create(EntityId.New(), EntityId.New(), "TEMP.4", "Temperature", TypedValueKind.Decimal, "C", 60, null, FixedClock.Now);
        var repository = new InMemoryTelemetryValueRepository(dataPoint);
        var service = new TelemetryValueService(repository, new FixedClock());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.ListHistoryAsync(dataPoint.Id.Value, FixedClock.Now.AddDays(-40), FixedClock.Now, 100, CancellationToken.None));
    }

    private sealed class FixedClock : IClock
    {
        public static readonly DateTimeOffset Now = new(2026, 7, 18, 12, 0, 0, TimeSpan.Zero);

        public DateTimeOffset UtcNow => Now;
    }

    private sealed class InMemoryTelemetryValueRepository(DataPoint dataPoint) : ITelemetryValueRepository
    {
        private CurrentValue? current;

        public List<HistoricalValue> History { get; } = [];

        public Task<DataPoint?> GetDataPointAsync(EntityId id, CancellationToken cancellationToken)
        {
            return Task.FromResult(dataPoint.Id == id ? dataPoint : null);
        }

        public Task<TelemetrySource?> GetTelemetrySourceAsync(EntityId id, CancellationToken cancellationToken)
        {
            return Task.FromResult<TelemetrySource?>(null);
        }

        public Task<CurrentValue?> GetCurrentValueForUpdateAsync(EntityId dataPointId, CancellationToken cancellationToken)
        {
            return Task.FromResult(current);
        }

        public Task<IReadOnlyList<CurrentValueWithDataPoint>> ListCurrentValuesAsync(EntityId? dataPointId, EntityId? equipmentId, EntityId? locationId, CancellationToken cancellationToken)
        {
            IReadOnlyList<CurrentValueWithDataPoint> result = current is null ? [] : [new CurrentValueWithDataPoint(current, dataPoint)];
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<HistoricalValue>> ListHistoryAsync(EntityId dataPointId, DateTimeOffset fromUtc, DateTimeOffset toUtc, int limit, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<HistoricalValue>>(History.Where(value => value.DataPointId == dataPointId).Take(limit).ToArray());
        }

        public void AddCurrentValue(CurrentValue currentValue)
        {
            current = currentValue;
        }

        public void AddHistoricalValue(HistoricalValue historicalValue)
        {
            History.Add(historicalValue);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
