using Dispatcher.Domain.Common;
using Dispatcher.Domain.Devices;

namespace Dispatcher.Domain.Tags;

public sealed class Tag : AggregateRoot
{
    public Guid DeviceId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public TagSourceType SourceType { get; private set; }

    public DeviceProtocol Protocol { get; private set; }

    public TagDataType DataType { get; private set; }

    public string? Unit { get; private set; }

    public double Scale { get; private set; } = 1;

    public double Offset { get; private set; }

    public int PollIntervalMs { get; private set; }

    public bool IsEnabled { get; private set; }

    public bool HistoryEnabled { get; private set; }

    public string? Description { get; private set; }

    public ModbusTagAddress? ModbusAddress { get; private set; }

    public SnmpTagAddress? SnmpAddress { get; private set; }

    private Tag()
    {
    }

    public static Tag CreateModbusTag(
        Guid deviceId,
        string name,
        string code,
        ModbusTagAddress address,
        TagDataType dataType,
        string? unit = null,
        double scale = 1,
        double offset = 0,
        int pollIntervalMs = 1000,
        bool historyEnabled = true,
        string? description = null)
    {
        var tag = new Tag
        {
            DeviceId = deviceId,
            Name = name,
            Code = code,
            SourceType = TagSourceType.Device,
            Protocol = DeviceProtocol.ModbusTcp,
            ModbusAddress = address,
            DataType = dataType,
            Unit = unit,
            Scale = scale,
            Offset = offset,
            PollIntervalMs = pollIntervalMs,
            HistoryEnabled = historyEnabled,
            Description = description,
            IsEnabled = true
        };

        tag.ValidateCommon();

        if (tag.ModbusAddress is null)
        {
            throw new DomainException("Modbus tag address is required.");
        }

        return tag;
    }

    public static Tag CreateSnmpTag(
        Guid deviceId,
        string name,
        string code,
        SnmpTagAddress address,
        TagDataType dataType,
        string? unit = null,
        double scale = 1,
        double offset = 0,
        int pollIntervalMs = 5000,
        bool historyEnabled = true,
        string? description = null)
    {
        var tag = new Tag
        {
            DeviceId = deviceId,
            Name = name,
            Code = code,
            SourceType = TagSourceType.Device,
            Protocol = DeviceProtocol.Snmp,
            SnmpAddress = address,
            DataType = dataType,
            Unit = unit,
            Scale = scale,
            Offset = offset,
            PollIntervalMs = pollIntervalMs,
            HistoryEnabled = historyEnabled,
            Description = description,
            IsEnabled = true
        };

        tag.ValidateCommon();

        if (tag.SnmpAddress is null)
        {
            throw new DomainException("SNMP tag address is required.");
        }

        return tag;
    }

    public double ApplyScale(double rawValue)
    {
        return rawValue * Scale + Offset;
    }

    public void Rename(string name)
    {
        Name = name;
        ValidateName();
        MarkUpdated();
    }

    public void ChangeDescription(string? description)
    {
        Description = description;
        MarkUpdated();
    }

    public void ChangeScaling(double scale, double offset)
    {
        Scale = scale;
        Offset = offset;
        MarkUpdated();
    }

    public void ChangePollInterval(int pollIntervalMs)
    {
        PollIntervalMs = pollIntervalMs;
        ValidatePollInterval();
        MarkUpdated();
    }

    public void EnableHistory()
    {
        HistoryEnabled = true;
        MarkUpdated();
    }

    public void DisableHistory()
    {
        HistoryEnabled = false;
        MarkUpdated();
    }

    public void Enable()
    {
        IsEnabled = true;
        MarkUpdated();
    }

    public void Disable()
    {
        IsEnabled = false;
        MarkUpdated();
    }

    private void ValidateCommon()
    {
        if (DeviceId == Guid.Empty)
        {
            throw new DomainException("Tag DeviceId is required.");
        }

        ValidateName();

        if (string.IsNullOrWhiteSpace(Code))
        {
            throw new DomainException("Tag code is required.");
        }

        ValidatePollInterval();
    }

    private void ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new DomainException("Tag name is required.");
        }
    }

    private void ValidatePollInterval()
    {
        if (PollIntervalMs < 100)
        {
            throw new DomainException("Tag poll interval must be at least 100 ms.");
        }
    }
}
