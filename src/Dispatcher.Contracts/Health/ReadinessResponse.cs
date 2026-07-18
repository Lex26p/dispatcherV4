namespace Dispatcher.Contracts.Health;

public sealed record ReadinessResponse(
    string Status,
    string Service,
    DateTimeOffset TimestampUtc,
    IReadOnlyList<HealthDependencyStatus> Dependencies);
