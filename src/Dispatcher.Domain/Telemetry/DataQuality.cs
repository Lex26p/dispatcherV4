namespace Dispatcher.Domain.Telemetry;

/// <summary>
/// Protocol-neutral quality of operator data. Quality is separate from alarm severity and freshness state.
/// </summary>
public enum DataQuality
{
    Initializing = 0,
    Good = 1,
    Uncertain = 2,
    Bad = 3,
    Stale = 4,
    Offline = 5,
}
