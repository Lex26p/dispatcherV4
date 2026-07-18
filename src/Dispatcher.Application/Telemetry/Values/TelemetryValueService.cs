using System.Globalization;
using Dispatcher.Application.Abstractions;
using Dispatcher.Contracts.Telemetry;
using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;

namespace Dispatcher.Application.Telemetry.Values;

public sealed class TelemetryValueService(
    ITelemetryValueRepository repository,
    IClock clock) : ITelemetryValueService
{
    private const int DefaultHistoryLimit = 500;
    private const int MaxHistoryLimit = 1000;
    private static readonly TimeSpan DefaultHistoryRange = TimeSpan.FromHours(1);
    private static readonly TimeSpan MaxHistoryRange = TimeSpan.FromDays(31);

    public async Task<IReadOnlyList<CurrentValueDto>> ListCurrentValuesAsync(Guid? dataPointId, Guid? equipmentId, Guid? locationId, CancellationToken cancellationToken)
    {
        var values = await repository.ListCurrentValuesAsync(
            dataPointId.HasValue ? EntityId.From(dataPointId.Value) : null,
            equipmentId.HasValue ? EntityId.From(equipmentId.Value) : null,
            locationId.HasValue ? EntityId.From(locationId.Value) : null,
            cancellationToken);

        return values
            .OrderBy(item => item.DataPoint.Code, StringComparer.OrdinalIgnoreCase)
            .Select(item => ToDto(item.Value, item.DataPoint))
            .ToArray();
    }

    public async Task<CurrentValueDto?> GetCurrentValueAsync(Guid dataPointId, CancellationToken cancellationToken)
    {
        var values = await repository.ListCurrentValuesAsync(EntityId.From(dataPointId), null, null, cancellationToken);
        var item = values.SingleOrDefault();
        return item is null ? null : ToDto(item.Value, item.DataPoint);
    }

    public async Task<UpsertCurrentValueResponse> UpsertCurrentValueAsync(UpsertCurrentValueRequest request, CancellationToken cancellationToken)
    {
        var dataPointId = EntityId.From(request.DataPointId);
        var dataPoint = await repository.GetDataPointAsync(dataPointId, cancellationToken);
        if (dataPoint is null || dataPoint.IsArchived)
        {
            throw new InvalidOperationException("Active data point is required.");
        }

        EntityId? telemetrySourceId = request.TelemetrySourceId.HasValue ? EntityId.From(request.TelemetrySourceId.Value) : null;
        if (telemetrySourceId.HasValue)
        {
            var source = await repository.GetTelemetrySourceAsync(telemetrySourceId.Value, cancellationToken);
            if (source is null || source.IsArchived)
            {
                throw new InvalidOperationException("Active telemetry source is required.");
            }
        }

        var valueKind = ParseValueKind(request.ValueKind);
        if (valueKind != dataPoint.ValueKind)
        {
            throw new InvalidOperationException("Current value kind must match DataPoint value kind.");
        }

        var quality = ParseQuality(request.Quality);
        var typedValue = ParseTypedValue(valueKind, request.RawValue, request.Unit ?? dataPoint.Unit);
        var sourceTimestampUtc = NormalizeUtc(request.SourceTimestampUtc ?? clock.UtcNow);
        var receivedAtUtc = NormalizeUtc(clock.UtcNow);

        var current = await repository.GetCurrentValueForUpdateAsync(dataPointId, cancellationToken);
        var applied = false;
        var result = "ignored_out_of_order";

        if (current is null)
        {
            current = CurrentValue.Create(dataPointId, telemetrySourceId, request.Sequence, typedValue, quality, sourceTimestampUtc, receivedAtUtc, request.ErrorMessage);
            repository.AddCurrentValue(current);
            applied = true;
            result = "created";
        }
        else if (current.TryApplySample(telemetrySourceId, request.Sequence, typedValue, quality, sourceTimestampUtc, receivedAtUtc, request.ErrorMessage))
        {
            applied = true;
            result = "updated";
        }

        if (applied)
        {
            repository.AddHistoricalValue(HistoricalValue.Create(EntityId.New(), dataPointId, telemetrySourceId, request.Sequence, typedValue, quality, sourceTimestampUtc, receivedAtUtc, request.ErrorMessage));
            await repository.SaveChangesAsync(cancellationToken);
        }

        return new UpsertCurrentValueResponse(applied, result, ToDto(current, dataPoint));
    }

    public async Task<IReadOnlyList<HistoricalValueDto>> ListHistoryAsync(Guid dataPointId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, int? limit, CancellationToken cancellationToken)
    {
        var to = NormalizeUtc(toUtc ?? clock.UtcNow);
        var from = NormalizeUtc(fromUtc ?? to.Subtract(DefaultHistoryRange));
        if (from >= to)
        {
            throw new ArgumentException("History range start must be earlier than range end.");
        }

        if (to - from > MaxHistoryRange)
        {
            throw new ArgumentOutOfRangeException(nameof(fromUtc), "History range cannot exceed 31 days in the baseline API.");
        }

        var effectiveLimit = limit ?? DefaultHistoryLimit;
        if (effectiveLimit <= 0 || effectiveLimit > MaxHistoryLimit)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), $"History limit must be between 1 and {MaxHistoryLimit}.");
        }

        var point = await repository.GetDataPointAsync(EntityId.From(dataPointId), cancellationToken);
        if (point is null || point.IsArchived)
        {
            throw new InvalidOperationException("Active data point is required.");
        }

        var values = await repository.ListHistoryAsync(point.Id, from, to, effectiveLimit, cancellationToken);
        return values.Select(ToDto).ToArray();
    }

    private CurrentValueDto ToDto(CurrentValue value, DataPoint dataPoint)
    {
        var age = NormalizeUtc(clock.UtcNow) - value.ReceivedAtUtc;
        var isFresh = age <= TimeSpan.FromSeconds(dataPoint.FreshnessTimeoutSeconds) && value.Quality is not DataQuality.Stale and not DataQuality.Offline;
        var freshness = isFresh ? FreshnessKind.Fresh.ToString() : FreshnessKind.Stale.ToString();

        return new CurrentValueDto(
            value.DataPointId.Value,
            value.TelemetrySourceId?.Value,
            value.Sequence,
            value.ValueKind.ToString(),
            value.RawValue,
            value.Unit,
            value.Quality.ToString(),
            freshness,
            isFresh,
            dataPoint.FreshnessTimeoutSeconds,
            value.SourceTimestampUtc,
            value.ReceivedAtUtc,
            value.UpdatedAtUtc,
            value.ErrorMessage);
    }

    private static HistoricalValueDto ToDto(HistoricalValue value) => new(
        value.Id.Value,
        value.DataPointId.Value,
        value.TelemetrySourceId?.Value,
        value.Sequence,
        value.ValueKind.ToString(),
        value.RawValue,
        value.Unit,
        value.Quality.ToString(),
        value.SourceTimestampUtc,
        value.ReceivedAtUtc,
        value.CreatedAtUtc,
        value.ErrorMessage);

    private static TypedValueKind ParseValueKind(string value)
    {
        if (!Enum.TryParse<TypedValueKind>(value, ignoreCase: true, out var kind))
        {
            throw new ArgumentException("Unknown typed value kind.", nameof(value));
        }

        return kind;
    }

    private static DataQuality ParseQuality(string value)
    {
        if (!Enum.TryParse<DataQuality>(value, ignoreCase: true, out var quality))
        {
            throw new ArgumentException("Unknown data quality.", nameof(value));
        }

        return quality;
    }

    private static TypedValue ParseTypedValue(TypedValueKind kind, string rawValue, string? unit) => kind switch
    {
        TypedValueKind.Boolean => TypedValue.FromBoolean(bool.Parse(rawValue), unit),
        TypedValueKind.Integer => TypedValue.FromInteger(long.Parse(rawValue, CultureInfo.InvariantCulture), unit),
        TypedValueKind.Decimal => TypedValue.FromDecimal(decimal.Parse(rawValue, CultureInfo.InvariantCulture), unit),
        TypedValueKind.Text => TypedValue.FromText(rawValue, unit),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), "Unsupported value kind.")
    };

    private static DateTimeOffset NormalizeUtc(DateTimeOffset value) => value.Offset == TimeSpan.Zero ? value : value.ToUniversalTime();
}
