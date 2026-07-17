namespace Dispatcher.Domain.Alarms;

public enum AlarmConditionType
{
    HighLimit = 1,
    LowLimit = 2,
    Equal = 3,
    NotEqual = 4,
    DeviceOffline = 5,
    TagBadQuality = 6,
    TagStale = 7
}
