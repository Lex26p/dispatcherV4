namespace Dispatcher.Web.Models;

public sealed record TagDto(
    Guid Id,
    Guid DeviceId,
    string Name,
    string Code,
    int SourceType,
    int Protocol,
    int DataType,
    string? Unit,
    double Scale,
    double Offset,
    int PollIntervalMs,
    bool IsEnabled,
    bool HistoryEnabled,
    string? Description,
    int? ModbusRegisterType,
    int? ModbusAddress,
    int? ModbusUnitId,
    int? ModbusByteOrder,
    int? ModbusWordOrder,
    string? SnmpOid);
