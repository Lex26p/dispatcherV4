namespace Dispatcher.Web.Models;

public sealed record TagValueDto(
    Guid TagId,
    Guid DeviceId,
    string? Value,
    int Quality,
    DateTimeOffset Timestamp,
    string? ErrorMessage);
