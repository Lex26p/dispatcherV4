using Dispatcher.Application.Abstractions;
using Dispatcher.Contracts.Telemetry;
using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;

namespace Dispatcher.Application.Telemetry.Configuration;

public sealed class TelemetryConfigurationService(
    ITelemetryConfigurationRepository repository,
    IClock clock) : ITelemetryConfigurationService
{
    public async Task<IReadOnlyList<TelemetrySourceDto>> ListSourcesAsync(bool includeArchived, CancellationToken cancellationToken)
    {
        var sources = await repository.ListSourcesAsync(includeArchived, cancellationToken);
        return sources.OrderBy(source => source.Code, StringComparer.OrdinalIgnoreCase).Select(ToDto).ToArray();
    }

    public async Task<TelemetrySourceDto?> GetSourceAsync(Guid id, CancellationToken cancellationToken)
    {
        var source = await repository.GetSourceAsync(EntityId.From(id), cancellationToken);
        return source is null ? null : ToDto(source);
    }

    public async Task<TelemetrySourceDto> CreateSourceAsync(CreateTelemetrySourceRequest request, CancellationToken cancellationToken)
    {
        var protocol = ParseProtocol(request.Protocol);
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var existing = await repository.GetSourceByCodeAsync(normalizedCode, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("Telemetry source code already exists.");
        }

        var source = TelemetrySource.Create(
            EntityId.New(),
            request.Code,
            request.Name,
            protocol,
            request.Endpoint,
            request.ConfigurationSchemaVersion,
            request.ConfigurationJson,
            request.SecretReference,
            request.Description,
            clock.UtcNow);

        repository.AddSource(source);
        await repository.SaveChangesAsync(cancellationToken);

        return ToDto(source);
    }

    public async Task<TelemetrySourceDto?> UpdateSourceAsync(Guid id, UpdateTelemetrySourceRequest request, CancellationToken cancellationToken)
    {
        var source = await repository.GetSourceAsync(EntityId.From(id), cancellationToken);
        if (source is null)
        {
            return null;
        }

        source.Rename(request.Name, request.Description, clock.UtcNow);
        source.ChangeConfiguration(
            request.Endpoint,
            request.ConfigurationSchemaVersion,
            request.ConfigurationJson,
            request.SecretReference,
            clock.UtcNow);

        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(source);
    }

    public async Task<TelemetrySourceDto?> EnableSourceAsync(Guid id, TelemetrySourceActionRequest request, CancellationToken cancellationToken)
    {
        var source = await repository.GetSourceAsync(EntityId.From(id), cancellationToken);
        if (source is null)
        {
            return null;
        }

        source.Enable(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(source);
    }

    public async Task<TelemetrySourceDto?> DisableSourceAsync(Guid id, TelemetrySourceActionRequest request, CancellationToken cancellationToken)
    {
        var source = await repository.GetSourceAsync(EntityId.From(id), cancellationToken);
        if (source is null)
        {
            return null;
        }

        source.Disable(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(source);
    }

    public async Task<TelemetrySourceDto?> ArchiveSourceAsync(Guid id, TelemetrySourceActionRequest request, CancellationToken cancellationToken)
    {
        var source = await repository.GetSourceAsync(EntityId.From(id), cancellationToken);
        if (source is null)
        {
            return null;
        }

        source.Archive(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(source);
    }

    public async Task<TelemetrySourceDto?> RestoreSourceAsync(Guid id, TelemetrySourceActionRequest request, CancellationToken cancellationToken)
    {
        var source = await repository.GetSourceAsync(EntityId.From(id), cancellationToken);
        if (source is null)
        {
            return null;
        }

        source.Restore(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(source);
    }

    public async Task<IReadOnlyList<DataPointDto>> ListDataPointsAsync(Guid? equipmentId, bool includeArchived, CancellationToken cancellationToken)
    {
        EntityId? typedEquipmentId = equipmentId.HasValue ? EntityId.From(equipmentId.Value) : null;
        var points = await repository.ListDataPointsAsync(typedEquipmentId, includeArchived, cancellationToken);
        return points.OrderBy(point => point.Code, StringComparer.OrdinalIgnoreCase).Select(ToDto).ToArray();
    }

    public async Task<DataPointDto?> GetDataPointAsync(Guid id, CancellationToken cancellationToken)
    {
        var point = await repository.GetDataPointAsync(EntityId.From(id), cancellationToken);
        return point is null ? null : ToDto(point);
    }

    public async Task<DataPointDto> CreateDataPointAsync(CreateDataPointRequest request, CancellationToken cancellationToken)
    {
        var equipmentId = EntityId.From(request.EquipmentId);
        if (!await repository.EquipmentExistsAsync(equipmentId, cancellationToken))
        {
            throw new InvalidOperationException("Equipment was not found or is archived.");
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var existing = await repository.GetDataPointByCodeAsync(normalizedCode, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("Data point code already exists.");
        }

        var point = DataPoint.Create(
            EntityId.New(),
            equipmentId,
            request.Code,
            request.Name,
            ParseValueKind(request.ValueKind),
            request.Unit,
            request.FreshnessTimeoutSeconds,
            request.Description,
            clock.UtcNow);

        repository.AddDataPoint(point);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(point);
    }

    public async Task<DataPointDto?> UpdateDataPointAsync(Guid id, UpdateDataPointRequest request, CancellationToken cancellationToken)
    {
        var point = await repository.GetDataPointAsync(EntityId.From(id), cancellationToken);
        if (point is null)
        {
            return null;
        }

        point.Rename(request.Name, request.Description, clock.UtcNow);
        point.ChangeUnit(request.Unit, clock.UtcNow);
        point.ChangeFreshnessPolicy(request.FreshnessTimeoutSeconds, clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(point);
    }

    public async Task<DataPointDto?> ArchiveDataPointAsync(Guid id, DataPointActionRequest request, CancellationToken cancellationToken)
    {
        var point = await repository.GetDataPointAsync(EntityId.From(id), cancellationToken);
        if (point is null)
        {
            return null;
        }

        point.Archive(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(point);
    }

    public async Task<DataPointDto?> RestoreDataPointAsync(Guid id, DataPointActionRequest request, CancellationToken cancellationToken)
    {
        var point = await repository.GetDataPointAsync(EntityId.From(id), cancellationToken);
        if (point is null)
        {
            return null;
        }

        if (!await repository.EquipmentExistsAsync(point.EquipmentId, cancellationToken))
        {
            throw new InvalidOperationException("Data point equipment was not found or is archived.");
        }

        point.Restore(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(point);
    }

    public async Task<IReadOnlyList<ProtocolMappingDto>> ListMappingsAsync(Guid? dataPointId, Guid? telemetrySourceId, bool includeArchived, CancellationToken cancellationToken)
    {
        EntityId? typedDataPointId = dataPointId.HasValue ? EntityId.From(dataPointId.Value) : null;
        EntityId? typedSourceId = telemetrySourceId.HasValue ? EntityId.From(telemetrySourceId.Value) : null;
        var mappings = await repository.ListMappingsAsync(typedDataPointId, typedSourceId, includeArchived, cancellationToken);
        return mappings.OrderBy(mapping => mapping.CreatedAtUtc).Select(ToDto).ToArray();
    }

    public async Task<ProtocolMappingDto?> GetMappingAsync(Guid id, CancellationToken cancellationToken)
    {
        var mapping = await repository.GetMappingAsync(EntityId.From(id), cancellationToken);
        return mapping is null ? null : ToDto(mapping);
    }

    public async Task<ProtocolMappingDto> CreateMappingAsync(CreateProtocolMappingRequest request, CancellationToken cancellationToken)
    {
        var dataPointId = EntityId.From(request.DataPointId);
        var sourceId = EntityId.From(request.TelemetrySourceId);
        var dataPoint = await repository.GetDataPointAsync(dataPointId, cancellationToken);
        var source = await repository.GetSourceAsync(sourceId, cancellationToken);
        if (dataPoint is null || dataPoint.IsArchived || source is null || source.IsArchived)
        {
            throw new InvalidOperationException("Active data point and telemetry source are required.");
        }

        var protocol = ParseProtocol(request.Protocol);
        if (source.Protocol != protocol)
        {
            throw new InvalidOperationException("Mapping protocol must match telemetry source protocol.");
        }

        var existing = await repository.GetMappingByDataPointAsync(dataPointId, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("Data point already has a protocol mapping.");
        }

        var mapping = ProtocolMapping.Create(
            EntityId.New(),
            dataPointId,
            sourceId,
            protocol,
            request.MappingSchemaVersion,
            request.MappingJson,
            clock.UtcNow);

        repository.AddMapping(mapping);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(mapping);
    }

    public async Task<ProtocolMappingDto?> UpdateMappingAsync(Guid id, UpdateProtocolMappingRequest request, CancellationToken cancellationToken)
    {
        var mapping = await repository.GetMappingAsync(EntityId.From(id), cancellationToken);
        if (mapping is null)
        {
            return null;
        }

        mapping.ChangeMapping(request.MappingSchemaVersion, request.MappingJson, clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(mapping);
    }

    public async Task<ProtocolMappingDto?> ArchiveMappingAsync(Guid id, ProtocolMappingActionRequest request, CancellationToken cancellationToken)
    {
        var mapping = await repository.GetMappingAsync(EntityId.From(id), cancellationToken);
        if (mapping is null)
        {
            return null;
        }

        mapping.Archive(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(mapping);
    }

    public async Task<ProtocolMappingDto?> RestoreMappingAsync(Guid id, ProtocolMappingActionRequest request, CancellationToken cancellationToken)
    {
        var mapping = await repository.GetMappingAsync(EntityId.From(id), cancellationToken);
        if (mapping is null)
        {
            return null;
        }

        mapping.Restore(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(mapping);
    }

    private static TelemetryProtocol ParseProtocol(string value)
    {
        if (!Enum.TryParse<TelemetryProtocol>(value, ignoreCase: true, out var protocol))
        {
            throw new ArgumentException("Unknown telemetry protocol.", nameof(value));
        }

        return protocol;
    }

    private static TypedValueKind ParseValueKind(string value)
    {
        if (!Enum.TryParse<TypedValueKind>(value, ignoreCase: true, out var kind))
        {
            throw new ArgumentException("Unknown typed value kind.", nameof(value));
        }

        return kind;
    }

    private static TelemetrySourceDto ToDto(TelemetrySource source) => new(
        source.Id.Value,
        source.Code,
        source.Name,
        source.Protocol.ToString(),
        source.Endpoint,
        source.ConfigurationSchemaVersion,
        source.ConfigurationJson,
        !string.IsNullOrWhiteSpace(source.SecretReference),
        MaskSecretReference(source.SecretReference),
        source.Description,
        source.IsEnabled,
        source.IsArchived,
        source.CreatedAtUtc,
        source.UpdatedAtUtc);

    private static DataPointDto ToDto(DataPoint point) => new(
        point.Id.Value,
        point.EquipmentId.Value,
        point.Code,
        point.Name,
        point.ValueKind.ToString(),
        point.Unit,
        point.FreshnessTimeoutSeconds,
        point.Description,
        point.IsArchived,
        point.CreatedAtUtc,
        point.UpdatedAtUtc);

    private static ProtocolMappingDto ToDto(ProtocolMapping mapping) => new(
        mapping.Id.Value,
        mapping.DataPointId.Value,
        mapping.TelemetrySourceId.Value,
        mapping.Protocol.ToString(),
        mapping.MappingSchemaVersion,
        mapping.MappingJson,
        mapping.IsArchived,
        mapping.CreatedAtUtc,
        mapping.UpdatedAtUtc);

    private static string? MaskSecretReference(string? secretReference)
    {
        if (string.IsNullOrWhiteSpace(secretReference))
        {
            return null;
        }

        return "secret://***";
    }
}
