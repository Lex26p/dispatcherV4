using Dispatcher.Domain.Tags;

namespace Dispatcher.Application.Tags;

public static class TagMapping
{
    public static TagDto ToDto(this Tag tag)
    {
        return new TagDto(
            tag.Id,
            tag.DeviceId,
            tag.Name,
            tag.Code,
            tag.SourceType,
            tag.Protocol,
            tag.DataType,
            tag.Unit,
            tag.Scale,
            tag.Offset,
            tag.PollIntervalMs,
            tag.IsEnabled,
            tag.HistoryEnabled,
            tag.Description,
            tag.ModbusAddress?.RegisterType,
            tag.ModbusAddress?.Address,
            tag.ModbusAddress?.UnitId,
            tag.ModbusAddress?.ByteOrder,
            tag.ModbusAddress?.WordOrder,
            tag.SnmpAddress?.Oid);
    }

    public static TagValueDto ToDto(this TagValue value)
    {
        return new TagValueDto(
            value.TagId,
            value.DeviceId,
            value.Value,
            value.Quality,
            value.Timestamp,
            value.ErrorMessage);
    }
}
