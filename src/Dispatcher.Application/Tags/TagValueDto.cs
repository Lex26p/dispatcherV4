using Dispatcher.Domain.Tags;

namespace Dispatcher.Application.Tags;

public sealed record TagValueDto(
    Guid TagId,
    Guid DeviceId,
    string? Value,
    TagQuality Quality,
    DateTimeOffset Timestamp,
    string? ErrorMessage);
