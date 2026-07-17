using Dispatcher.Application.Tags.Commands;
using Dispatcher.Domain.Tags;

namespace Dispatcher.Api.Contracts.Tags;

public sealed class CreateModbusTagRequest
{
    public Guid DeviceId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public ModbusRegisterType RegisterType { get; init; } = ModbusRegisterType.HoldingRegister;

    public int Address { get; init; }

    public int UnitId { get; init; } = 1;

    public TagDataType DataType { get; init; } = TagDataType.Int16;

    public string? Unit { get; init; }

    public double Scale { get; init; } = 1;

    public double Offset { get; init; }

    public int PollIntervalMs { get; init; } = 1000;

    public bool HistoryEnabled { get; init; } = true;

    public ByteOrder ByteOrder { get; init; } = ByteOrder.BigEndian;

    public WordOrder WordOrder { get; init; } = WordOrder.BigEndian;

    public string? Description { get; init; }

    public CreateModbusTagCommand ToCommand()
    {
        return new CreateModbusTagCommand(
            DeviceId,
            Name,
            Code,
            RegisterType,
            Address,
            UnitId,
            DataType,
            Unit,
            Scale,
            Offset,
            PollIntervalMs,
            HistoryEnabled,
            ByteOrder,
            WordOrder,
            Description);
    }
}
