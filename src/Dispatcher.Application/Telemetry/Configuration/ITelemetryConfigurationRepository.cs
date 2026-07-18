using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;

namespace Dispatcher.Application.Telemetry.Configuration;

public interface ITelemetryConfigurationRepository
{
    Task<IReadOnlyList<TelemetrySource>> ListSourcesAsync(bool includeArchived, CancellationToken cancellationToken);

    Task<TelemetrySource?> GetSourceAsync(EntityId id, CancellationToken cancellationToken);

    Task<TelemetrySource?> GetSourceByCodeAsync(string code, CancellationToken cancellationToken);

    void AddSource(TelemetrySource source);

    Task<IReadOnlyList<DataPoint>> ListDataPointsAsync(EntityId? equipmentId, bool includeArchived, CancellationToken cancellationToken);

    Task<DataPoint?> GetDataPointAsync(EntityId id, CancellationToken cancellationToken);

    Task<DataPoint?> GetDataPointByCodeAsync(string code, CancellationToken cancellationToken);

    void AddDataPoint(DataPoint dataPoint);

    Task<IReadOnlyList<ProtocolMapping>> ListMappingsAsync(EntityId? dataPointId, EntityId? telemetrySourceId, bool includeArchived, CancellationToken cancellationToken);

    Task<ProtocolMapping?> GetMappingAsync(EntityId id, CancellationToken cancellationToken);

    Task<ProtocolMapping?> GetMappingByDataPointAsync(EntityId dataPointId, CancellationToken cancellationToken);

    void AddMapping(ProtocolMapping mapping);

    Task<bool> EquipmentExistsAsync(EntityId equipmentId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
