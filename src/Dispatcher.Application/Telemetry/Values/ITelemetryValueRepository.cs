using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;

namespace Dispatcher.Application.Telemetry.Values;

public interface ITelemetryValueRepository
{
    Task<DataPoint?> GetDataPointAsync(EntityId id, CancellationToken cancellationToken);

    Task<TelemetrySource?> GetTelemetrySourceAsync(EntityId id, CancellationToken cancellationToken);

    Task<CurrentValue?> GetCurrentValueForUpdateAsync(EntityId dataPointId, CancellationToken cancellationToken);

    Task<IReadOnlyList<CurrentValueWithDataPoint>> ListCurrentValuesAsync(EntityId? dataPointId, EntityId? equipmentId, EntityId? locationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<HistoricalValue>> ListHistoryAsync(EntityId dataPointId, DateTimeOffset fromUtc, DateTimeOffset toUtc, int limit, CancellationToken cancellationToken);

    void AddCurrentValue(CurrentValue currentValue);

    void AddHistoricalValue(HistoricalValue historicalValue);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
