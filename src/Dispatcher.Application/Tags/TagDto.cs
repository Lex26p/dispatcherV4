using Dispatcher.Domain.Devices;
using Dispatcher.Domain.Tags;

namespace Dispatcher.Application.Tags;

public sealed record TagDto(
    Guid Id,
    Guid DeviceId,
    string Name,
    string Code,
    TagSourceType SourceType,
    DeviceProtocol Protocol,
    TagDataType DataType,
    string? Unit,
    double Scale,
    double Offset,
    int PollIntervalMs,
    bool IsEnabled,
    bool HistoryEnabled,
    string? Description,
    ModbusRegisterType? ModbusRegisterType,
    int? ModbusAddress,
    int? ModbusUnitId,
    ByteOrder? ModbusByteOrder,
    WordOrder? ModbusWordOrder,
    string? SnmpOid);
