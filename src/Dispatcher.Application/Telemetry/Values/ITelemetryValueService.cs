using Dispatcher.Contracts.Telemetry;

namespace Dispatcher.Application.Telemetry.Values;

public interface ITelemetryValueService
{
    Task<IReadOnlyList<CurrentValueDto>> ListCurrentValuesAsync(Guid? dataPointId, Guid? equipmentId, Guid? locationId, CancellationToken cancellationToken);

    Task<CurrentValueDto?> GetCurrentValueAsync(Guid dataPointId, CancellationToken cancellationToken);

    Task<UpsertCurrentValueResponse> UpsertCurrentValueAsync(UpsertCurrentValueRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<HistoricalValueDto>> ListHistoryAsync(Guid dataPointId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, int? limit, CancellationToken cancellationToken);
}
