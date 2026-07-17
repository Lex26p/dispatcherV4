namespace Dispatcher.Application.Abstractions;

public sealed class CorrelationContext : ICorrelationContext
{
    public string CorrelationId { get; set; } = string.Empty;
}
