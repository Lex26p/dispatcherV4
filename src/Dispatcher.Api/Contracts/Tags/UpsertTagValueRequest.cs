using Dispatcher.Application.Tags;
using Dispatcher.Domain.Tags;

namespace Dispatcher.Api.Contracts.Tags;

public sealed class UpsertTagValueRequest
{
    public Guid TagId { get; init; }

    public Guid DeviceId { get; init; }

    public string? Value { get; init; }

    public TagQuality Quality { get; init; } = TagQuality.Good;

    public DateTimeOffset? Timestamp { get; init; }

    public string? ErrorMessage { get; init; }

    public TagValueDto ToDto(Guid? routeTagId = null)
    {
        var effectiveTagId = routeTagId ?? TagId;

        return new TagValueDto(
            effectiveTagId,
            DeviceId,
            Value,
            Quality,
            Timestamp ?? DateTimeOffset.UtcNow,
            ErrorMessage);
    }
}
