using Dispatcher.Domain.Common;

namespace Dispatcher.Domain.Telemetry;

public enum FreshnessKind
{
    Initializing = 0,
    Fresh = 1,
    Stale = 2,
    Offline = 3,
}

/// <summary>
/// Freshness of a value relative to its receive/source timestamps and source state.
/// </summary>
public sealed record FreshnessState
{
    private FreshnessState(FreshnessKind kind, UtcTimestamp? changedAt, string? reason)
    {
        Kind = kind;
        ChangedAt = changedAt;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }

    public FreshnessKind Kind { get; }

    public UtcTimestamp? ChangedAt { get; }

    public string? Reason { get; }

    public bool AllowsOperatorTrust => Kind == FreshnessKind.Fresh;

    public static FreshnessState Initializing(string? reason = null) => new(FreshnessKind.Initializing, null, reason);

    public static FreshnessState Fresh(UtcTimestamp changedAt, string? reason = null) => new(FreshnessKind.Fresh, changedAt, reason);

    public static FreshnessState Stale(UtcTimestamp changedAt, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Stale state requires a reason.", nameof(reason));
        }

        return new FreshnessState(FreshnessKind.Stale, changedAt, reason);
    }

    public static FreshnessState Offline(UtcTimestamp changedAt, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Offline state requires a reason.", nameof(reason));
        }

        return new FreshnessState(FreshnessKind.Offline, changedAt, reason);
    }
}
