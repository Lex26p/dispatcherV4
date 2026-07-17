using Dispatcher.Application.Tags.Commands;
using Dispatcher.Domain.Tags;

namespace Dispatcher.Api.Contracts.Tags;

public sealed class CreateSnmpTagRequest
{
    public Guid DeviceId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public string Oid { get; init; } = string.Empty;

    public TagDataType DataType { get; init; } = TagDataType.String;

    public string? Unit { get; init; }

    public double Scale { get; init; } = 1;

    public double Offset { get; init; }

    public int PollIntervalMs { get; init; } = 5000;

    public bool HistoryEnabled { get; init; } = true;

    public string? Description { get; init; }

    public CreateSnmpTagCommand ToCommand()
    {
        return new CreateSnmpTagCommand(
            DeviceId,
            Name,
            Code,
            Oid,
            DataType,
            Unit,
            Scale,
            Offset,
            PollIntervalMs,
            HistoryEnabled,
            Description);
    }
}
