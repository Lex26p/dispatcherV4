using Dispatcher.Application.Abstractions;

namespace Dispatcher.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
