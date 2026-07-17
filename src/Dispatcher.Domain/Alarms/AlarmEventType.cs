namespace Dispatcher.Domain.Alarms;

public enum AlarmEventType
{
    Raised = 1,
    Acknowledged = 2,
    Cleared = 3,
    Closed = 4
}
