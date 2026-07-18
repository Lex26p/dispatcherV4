using Dispatcher.Domain.Common;

namespace Dispatcher.Domain.Telemetry;

/// <summary>
/// Committed current snapshot for a DataPoint. SignalR updates are not the source of truth; this row is.
/// </summary>
public sealed class CurrentValue
{
    private CurrentValue()
    {
    }

    private CurrentValue(
        EntityId dataPointId,
        EntityId? telemetrySourceId,
        long sequence,
        TypedValue value,
        DataQuality quality,
        DateTimeOffset sourceTimestampUtc,
        DateTimeOffset receivedAtUtc,
        string? errorMessage)
    {
        DataPointId = dataPointId;
        TelemetrySourceId = telemetrySourceId;
        Sequence = ValidateSequence(sequence);
        ValueKind = value.Kind;
        RawValue = NormalizeRequired(value.RawValue, nameof(value.RawValue), 2000);
        Unit = NormalizeOptional(value.Unit, 40);
        Quality = quality;
        SourceTimestampUtc = EnsureUtc(sourceTimestampUtc, nameof(sourceTimestampUtc));
        ReceivedAtUtc = EnsureUtc(receivedAtUtc, nameof(receivedAtUtc));
        UpdatedAtUtc = ReceivedAtUtc;
        ErrorMessage = NormalizeOptional(errorMessage, 1000);
    }

    public EntityId DataPointId { get; private set; }

    public EntityId? TelemetrySourceId { get; private set; }

    public long Sequence { get; private set; }

    public TypedValueKind ValueKind { get; private set; }

    public string RawValue { get; private set; } = string.Empty;

    public string? Unit { get; private set; }

    public DataQuality Quality { get; private set; }

    public DateTimeOffset SourceTimestampUtc { get; private set; }

    public DateTimeOffset ReceivedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public string? ErrorMessage { get; private set; }

    public bool IsGood => Quality == DataQuality.Good;

    public TypedValue ToTypedValue() => ValueKind switch
    {
        TypedValueKind.Boolean => TypedValue.FromBoolean(bool.Parse(RawValue), Unit),
        TypedValueKind.Integer => TypedValue.FromInteger(long.Parse(RawValue, System.Globalization.CultureInfo.InvariantCulture), Unit),
        TypedValueKind.Decimal => TypedValue.FromDecimal(decimal.Parse(RawValue, System.Globalization.CultureInfo.InvariantCulture), Unit),
        TypedValueKind.Text => TypedValue.FromText(RawValue, Unit),
        _ => throw new InvalidOperationException("Unsupported typed value kind.")
    };

    public static CurrentValue Create(
        EntityId dataPointId,
        EntityId? telemetrySourceId,
        long sequence,
        TypedValue value,
        DataQuality quality,
        DateTimeOffset sourceTimestampUtc,
        DateTimeOffset receivedAtUtc,
        string? errorMessage = null)
    {
        return new CurrentValue(dataPointId, telemetrySourceId, sequence, value, quality, sourceTimestampUtc, receivedAtUtc, errorMessage);
    }

    public bool TryApplySample(
        EntityId? telemetrySourceId,
        long sequence,
        TypedValue value,
        DataQuality quality,
        DateTimeOffset sourceTimestampUtc,
        DateTimeOffset receivedAtUtc,
        string? errorMessage = null)
    {
        var validatedSequence = ValidateSequence(sequence);
        if (validatedSequence <= Sequence)
        {
            return false;
        }

        TelemetrySourceId = telemetrySourceId;
        Sequence = validatedSequence;
        ValueKind = value.Kind;
        RawValue = NormalizeRequired(value.RawValue, nameof(value.RawValue), 2000);
        Unit = NormalizeOptional(value.Unit, 40);
        Quality = quality;
        SourceTimestampUtc = EnsureUtc(sourceTimestampUtc, nameof(sourceTimestampUtc));
        ReceivedAtUtc = EnsureUtc(receivedAtUtc, nameof(receivedAtUtc));
        UpdatedAtUtc = ReceivedAtUtc;
        ErrorMessage = NormalizeOptional(errorMessage, 1000);
        return true;
    }

    private static long ValidateSequence(long sequence)
    {
        if (sequence <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequence), "Sample sequence must be positive.");
        }

        return sequence;
    }

    private static DateTimeOffset EnsureUtc(DateTimeOffset value, string parameterName)
    {
        return value.Offset == TimeSpan.Zero
            ? value
            : value.ToUniversalTime();
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
