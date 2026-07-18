using Dispatcher.Application.Telemetry.Configuration;
using Dispatcher.Domain.Assets;
using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;
using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.Infrastructure.Telemetry;

public sealed class EfTelemetryConfigurationRepository(DispatcherDbContext dbContext) : ITelemetryConfigurationRepository
{
    public async Task<IReadOnlyList<TelemetrySource>> ListSourcesAsync(bool includeArchived, CancellationToken cancellationToken)
    {
        var query = dbContext.TelemetrySources.AsQueryable();
        if (!includeArchived)
        {
            query = query.Where(source => !source.IsArchived);
        }

        return await query.OrderBy(source => source.Code).ToArrayAsync(cancellationToken);
    }

    public Task<TelemetrySource?> GetSourceAsync(EntityId id, CancellationToken cancellationToken)
    {
        return dbContext.TelemetrySources.SingleOrDefaultAsync(source => source.Id == id, cancellationToken);
    }

    public Task<TelemetrySource?> GetSourceByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return dbContext.TelemetrySources.SingleOrDefaultAsync(source => source.Code == normalizedCode, cancellationToken);
    }

    public void AddSource(TelemetrySource source)
    {
        dbContext.TelemetrySources.Add(source);
    }

    public async Task<IReadOnlyList<DataPoint>> ListDataPointsAsync(EntityId? equipmentId, bool includeArchived, CancellationToken cancellationToken)
    {
        var query = dbContext.DataPoints.AsQueryable();
        if (equipmentId.HasValue)
        {
            query = query.Where(point => point.EquipmentId == equipmentId.Value);
        }

        if (!includeArchived)
        {
            query = query.Where(point => !point.IsArchived);
        }

        return await query.OrderBy(point => point.Code).ToArrayAsync(cancellationToken);
    }

    public Task<DataPoint?> GetDataPointAsync(EntityId id, CancellationToken cancellationToken)
    {
        return dbContext.DataPoints.SingleOrDefaultAsync(point => point.Id == id, cancellationToken);
    }

    public Task<DataPoint?> GetDataPointByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return dbContext.DataPoints.SingleOrDefaultAsync(point => point.Code == normalizedCode, cancellationToken);
    }

    public void AddDataPoint(DataPoint dataPoint)
    {
        dbContext.DataPoints.Add(dataPoint);
    }

    public async Task<IReadOnlyList<ProtocolMapping>> ListMappingsAsync(EntityId? dataPointId, EntityId? telemetrySourceId, bool includeArchived, CancellationToken cancellationToken)
    {
        var query = dbContext.ProtocolMappings.AsQueryable();
        if (dataPointId.HasValue)
        {
            query = query.Where(mapping => mapping.DataPointId == dataPointId.Value);
        }

        if (telemetrySourceId.HasValue)
        {
            query = query.Where(mapping => mapping.TelemetrySourceId == telemetrySourceId.Value);
        }

        if (!includeArchived)
        {
            query = query.Where(mapping => !mapping.IsArchived);
        }

        return await query.OrderBy(mapping => mapping.CreatedAtUtc).ToArrayAsync(cancellationToken);
    }

    public Task<ProtocolMapping?> GetMappingAsync(EntityId id, CancellationToken cancellationToken)
    {
        return dbContext.ProtocolMappings.SingleOrDefaultAsync(mapping => mapping.Id == id, cancellationToken);
    }

    public Task<ProtocolMapping?> GetMappingByDataPointAsync(EntityId dataPointId, CancellationToken cancellationToken)
    {
        return dbContext.ProtocolMappings.SingleOrDefaultAsync(mapping => mapping.DataPointId == dataPointId && !mapping.IsArchived, cancellationToken);
    }

    public void AddMapping(ProtocolMapping mapping)
    {
        dbContext.ProtocolMappings.Add(mapping);
    }

    public Task<bool> EquipmentExistsAsync(EntityId equipmentId, CancellationToken cancellationToken)
    {
        return dbContext.Set<Equipment>().AnyAsync(equipment => equipment.Id == equipmentId && !equipment.IsArchived, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
