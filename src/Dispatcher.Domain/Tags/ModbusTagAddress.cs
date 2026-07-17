using Dispatcher.Domain.Common;

namespace Dispatcher.Domain.Tags;

public sealed class ModbusTagAddress
{
    public ModbusRegisterType RegisterType { get; private set; }

    public int Address { get; private set; }

    public int UnitId { get; private set; }

    public ByteOrder ByteOrder { get; private set; }

    public WordOrder WordOrder { get; private set; }

    private ModbusTagAddress()
    {
    }

    public ModbusTagAddress(
        ModbusRegisterType registerType,
        int address,
        int unitId = 1,
        ByteOrder byteOrder = ByteOrder.BigEndian,
        WordOrder wordOrder = WordOrder.BigEndian)
    {
        RegisterType = registerType;
        Address = address;
        UnitId = unitId;
        ByteOrder = byteOrder;
        WordOrder = wordOrder;

        Validate();
    }

    private void Validate()
    {
        if (Address < 0)
        {
            throw new DomainException("Modbus address cannot be negative.");
        }

        if (UnitId is < 0 or > 255)
        {
            throw new DomainException("Modbus Unit ID must be between 0 and 255.");
        }
    }
}
