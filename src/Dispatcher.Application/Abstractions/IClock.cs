namespace Dispatcher.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
