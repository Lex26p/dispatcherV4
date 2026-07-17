namespace Dispatcher.Contracts.Common;

public sealed record RequestMetadata(
    string CorrelationId,
    DateTimeOffset TimestampUtc);
