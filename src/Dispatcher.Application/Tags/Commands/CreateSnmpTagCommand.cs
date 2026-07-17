using Dispatcher.Domain.Tags;

namespace Dispatcher.Application.Tags.Commands;

public sealed record CreateSnmpTagCommand(
    Guid DeviceId,
    string Name,
    string Code,
    string Oid,
    TagDataType DataType,
    string? Unit,
    double Scale,
    double Offset,
    int PollIntervalMs,
    bool HistoryEnabled,
    string? Description);
