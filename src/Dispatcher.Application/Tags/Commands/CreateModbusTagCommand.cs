using Dispatcher.Domain.Tags;

namespace Dispatcher.Application.Tags.Commands;

public sealed record CreateModbusTagCommand(
    Guid DeviceId,
    string Name,
    string Code,
    ModbusRegisterType RegisterType,
    int Address,
    int UnitId,
    TagDataType DataType,
    string? Unit,
    double Scale,
    double Offset,
    int PollIntervalMs,
    bool HistoryEnabled,
    ByteOrder ByteOrder,
    WordOrder WordOrder,
    string? Description);
