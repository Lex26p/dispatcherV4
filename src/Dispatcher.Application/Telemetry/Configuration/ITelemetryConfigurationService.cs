using Dispatcher.Contracts.Telemetry;

namespace Dispatcher.Application.Telemetry.Configuration;

public interface ITelemetryConfigurationService
{
    Task<IReadOnlyList<TelemetrySourceDto>> ListSourcesAsync(bool includeArchived, CancellationToken cancellationToken);

    Task<TelemetrySourceDto?> GetSourceAsync(Guid id, CancellationToken cancellationToken);

    Task<TelemetrySourceDto> CreateSourceAsync(CreateTelemetrySourceRequest request, CancellationToken cancellationToken);

    Task<TelemetrySourceDto?> UpdateSourceAsync(Guid id, UpdateTelemetrySourceRequest request, CancellationToken cancellationToken);

    Task<TelemetrySourceDto?> EnableSourceAsync(Guid id, TelemetrySourceActionRequest request, CancellationToken cancellationToken);

    Task<TelemetrySourceDto?> DisableSourceAsync(Guid id, TelemetrySourceActionRequest request, CancellationToken cancellationToken);

    Task<TelemetrySourceDto?> ArchiveSourceAsync(Guid id, TelemetrySourceActionRequest request, CancellationToken cancellationToken);

    Task<TelemetrySourceDto?> RestoreSourceAsync(Guid id, TelemetrySourceActionRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<DataPointDto>> ListDataPointsAsync(Guid? equipmentId, bool includeArchived, CancellationToken cancellationToken);

    Task<DataPointDto?> GetDataPointAsync(Guid id, CancellationToken cancellationToken);

    Task<DataPointDto> CreateDataPointAsync(CreateDataPointRequest request, CancellationToken cancellationToken);

    Task<DataPointDto?> UpdateDataPointAsync(Guid id, UpdateDataPointRequest request, CancellationToken cancellationToken);

    Task<DataPointDto?> ArchiveDataPointAsync(Guid id, DataPointActionRequest request, CancellationToken cancellationToken);

    Task<DataPointDto?> RestoreDataPointAsync(Guid id, DataPointActionRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProtocolMappingDto>> ListMappingsAsync(Guid? dataPointId, Guid? telemetrySourceId, bool includeArchived, CancellationToken cancellationToken);

    Task<ProtocolMappingDto?> GetMappingAsync(Guid id, CancellationToken cancellationToken);

    Task<ProtocolMappingDto> CreateMappingAsync(CreateProtocolMappingRequest request, CancellationToken cancellationToken);

    Task<ProtocolMappingDto?> UpdateMappingAsync(Guid id, UpdateProtocolMappingRequest request, CancellationToken cancellationToken);

    Task<ProtocolMappingDto?> ArchiveMappingAsync(Guid id, ProtocolMappingActionRequest request, CancellationToken cancellationToken);

    Task<ProtocolMappingDto?> RestoreMappingAsync(Guid id, ProtocolMappingActionRequest request, CancellationToken cancellationToken);
}
