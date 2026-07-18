namespace Dispatcher.Contracts.Health;

public sealed record HealthDependencyStatus(
    string Name,
    string Status,
    string? Details = null);
