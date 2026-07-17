using Dispatcher.Application.Abstractions.System;

namespace Dispatcher.Infrastructure.System;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
