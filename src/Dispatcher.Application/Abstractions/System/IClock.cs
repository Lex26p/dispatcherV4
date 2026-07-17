namespace Dispatcher.Application.Abstractions.System;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
