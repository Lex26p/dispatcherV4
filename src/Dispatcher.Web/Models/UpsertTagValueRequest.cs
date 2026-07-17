namespace Dispatcher.Web.Models;

public sealed class UpsertTagValueRequest
{
    public Guid TagId { get; init; }

    public Guid DeviceId { get; init; }

    public string? Value { get; init; }

    public int Quality { get; init; } = 1;

    public DateTimeOffset? Timestamp { get; init; }

    public string? ErrorMessage { get; init; }
}
