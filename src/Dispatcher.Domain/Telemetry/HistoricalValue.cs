using Dispatcher.Domain.Common;

namespace Dispatcher.Domain.Telemetry;

/// <summary>
/// Append-only telemetry sample history. Business workflows must not update historical samples in place.
/// </summary>
public sealed class HistoricalValue
{
    private HistoricalValue()
    {
    }

    private HistoricalValue(
        EntityId id,
        EntityId dataPointId,
        EntityId? telemetrySourceId,
        long sequence,
        TypedValue value,
        DataQuality quality,
        DateTimeOffset sourceTimestampUtc,
        DateTimeOffset receivedAtUtc,
        string? errorMessage)
    {
        Id = id;
        DataPointId = dataPointId;
        TelemetrySourceId = telemetrySourceId;
        Sequence = ValidateSequence(sequence);
        ValueKind = value.Kind;
        RawValue = NormalizeRequired(value.RawValue, nameof(value.RawValue), 2000);
        Unit = NormalizeOptional(value.Unit, 40);
        Quality = quality;
        SourceTimestampUtc = sourceTimestampUtc.ToUniversalTime();
        ReceivedAtUtc = receivedAtUtc.ToUniversalTime();
        ErrorMessage = NormalizeOptional(errorMessage, 1000);
        CreatedAtUtc = ReceivedAtUtc;
    }

    public EntityId Id { get; private set; }

    public EntityId DataPointId { get; private set; }

    public EntityId? TelemetrySourceId { get; private set; }

    public long Sequence { get; private set; }

    public TypedValueKind ValueKind { get; private set; }

    public string RawValue { get; private set; } = string.Empty;

    public string? Unit { get; private set; }

    public DataQuality Quality { get; private set; }

    public DateTimeOffset SourceTimestampUtc { get; private set; }

    public DateTimeOffset ReceivedAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public string? ErrorMessage { get; private set; }

    public static HistoricalValue Create(
        EntityId id,
        EntityId dataPointId,
        EntityId? telemetrySourceId,
        long sequence,
        TypedValue value,
        DataQuality quality,
        DateTimeOffset sourceTimestampUtc,
        DateTimeOffset receivedAtUtc,
        string? errorMessage = null)
    {
        return new HistoricalValue(id, dataPointId, telemetrySourceId, sequence, value, quality, sourceTimestampUtc, receivedAtUtc, errorMessage);
    }

    private static long ValidateSequence(long sequence)
    {
        if (sequence <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequence), "Sample sequence must be positive.");
        }

        return sequence;
    }

    private static string NormalizeRequired(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameterName);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", nameof(value));
        }

        return normalized;
    }
}
