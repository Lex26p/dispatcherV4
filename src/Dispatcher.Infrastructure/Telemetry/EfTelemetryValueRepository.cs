using Dispatcher.Application.Telemetry.Values;
using Dispatcher.Domain.Assets;
using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;
using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.Infrastructure.Telemetry;

public sealed class EfTelemetryValueRepository(DispatcherDbContext dbContext) : ITelemetryValueRepository
{
    public Task<DataPoint?> GetDataPointAsync(EntityId id, CancellationToken cancellationToken)
    {
        return dbContext.DataPoints.SingleOrDefaultAsync(point => point.Id == id, cancellationToken);
    }

    public Task<TelemetrySource?> GetTelemetrySourceAsync(EntityId id, CancellationToken cancellationToken)
    {
        return dbContext.TelemetrySources.SingleOrDefaultAsync(source => source.Id == id, cancellationToken);
    }

    public Task<CurrentValue?> GetCurrentValueForUpdateAsync(EntityId dataPointId, CancellationToken cancellationToken)
    {
        return dbContext.CurrentValues.SingleOrDefaultAsync(value => value.DataPointId == dataPointId, cancellationToken);
    }

    public async Task<IReadOnlyList<CurrentValueWithDataPoint>> ListCurrentValuesAsync(EntityId? dataPointId, EntityId? equipmentId, EntityId? locationId, CancellationToken cancellationToken)
    {
        var query = from current in dbContext.CurrentValues.AsNoTracking()
                    join point in dbContext.DataPoints.AsNoTracking() on current.DataPointId equals point.Id
                    join equipment in dbContext.Set<Equipment>().AsNoTracking() on point.EquipmentId equals equipment.Id
                    where !point.IsArchived && !equipment.IsArchived
                    select new { current, point, equipment };

        if (dataPointId.HasValue)
        {
            query = query.Where(item => item.point.Id == dataPointId.Value);
        }

        if (equipmentId.HasValue)
        {
            query = query.Where(item => item.point.EquipmentId == equipmentId.Value);
        }

        if (locationId.HasValue)
        {
            query = query.Where(item => item.equipment.LocationId == locationId.Value);
        }

        return await query
            .OrderBy(item => item.point.Code)
            .Select(item => new CurrentValueWithDataPoint(item.current, item.point))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HistoricalValue>> ListHistoryAsync(EntityId dataPointId, DateTimeOffset fromUtc, DateTimeOffset toUtc, int limit, CancellationToken cancellationToken)
    {
        return await dbContext.HistoricalValues
            .AsNoTracking()
            .Where(value => value.DataPointId == dataPointId && value.SourceTimestampUtc >= fromUtc && value.SourceTimestampUtc <= toUtc)
            .OrderByDescending(value => value.SourceTimestampUtc)
            .Take(limit)
            .OrderBy(value => value.SourceTimestampUtc)
            .ToArrayAsync(cancellationToken);
    }

    public void AddCurrentValue(CurrentValue currentValue)
    {
        dbContext.CurrentValues.Add(currentValue);
    }

    public void AddHistoricalValue(HistoricalValue historicalValue)
    {
        dbContext.HistoricalValues.Add(historicalValue);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
