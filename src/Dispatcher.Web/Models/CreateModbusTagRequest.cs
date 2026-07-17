namespace Dispatcher.Web.Models;

public sealed class CreateModbusTagRequest
{
    public Guid DeviceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public int RegisterType { get; set; } = 4;

    public int Address { get; set; }

    public int UnitId { get; set; } = 1;

    public int DataType { get; set; } = 2;

    public string? Unit { get; set; }

    public double Scale { get; set; } = 1;

    public double Offset { get; set; }

    public int PollIntervalMs { get; set; } = 1000;

    public bool HistoryEnabled { get; set; } = true;

    public int ByteOrder { get; set; } = 1;

    public int WordOrder { get; set; } = 1;

    public string? Description { get; set; }
}
