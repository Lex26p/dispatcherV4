namespace Dispatcher.Web.Models;

public sealed class CreateSnmpTagRequest
{
    public Guid DeviceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Oid { get; set; } = string.Empty;

    public int DataType { get; set; } = 8;

    public string? Unit { get; set; }

    public double Scale { get; set; } = 1;

    public double Offset { get; set; }

    public int PollIntervalMs { get; set; } = 5000;

    public bool HistoryEnabled { get; set; } = true;

    public string? Description { get; set; }
}
