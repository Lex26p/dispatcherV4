using Dispatcher.Domain.Common;

namespace Dispatcher.Domain.Tags;

public sealed class TagValue : Entity
{
    public Guid TagId { get; private set; }

    public Guid DeviceId { get; private set; }

    public string? Value { get; private set; }

    public TagQuality Quality { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public string? ErrorMessage { get; private set; }

    public bool IsGood => Quality == TagQuality.Good;

    private TagValue()
    {
    }

    public TagValue(
        Guid tagId,
        Guid deviceId,
        string? value,
        TagQuality quality,
        DateTimeOffset timestamp,
        string? errorMessage = null)
    {
        if (tagId == Guid.Empty)
        {
            throw new DomainException("TagValue TagId is required.");
        }

        if (deviceId == Guid.Empty)
        {
            throw new DomainException("TagValue DeviceId is required.");
        }

        TagId = tagId;
        DeviceId = deviceId;
        Value = value;
        Quality = quality;
        Timestamp = timestamp;
        ErrorMessage = errorMessage;
    }

    public static TagValue Good(Guid tagId, Guid deviceId, string? value, DateTimeOffset timestamp)
    {
        return new TagValue(tagId, deviceId, value, TagQuality.Good, timestamp);
    }

    public static TagValue Bad(Guid tagId, Guid deviceId, DateTimeOffset timestamp, string errorMessage)
    {
        return new TagValue(tagId, deviceId, null, TagQuality.Bad, timestamp, errorMessage);
    }
}
