using Dispatcher.Application.Tags;
using Microsoft.AspNetCore.SignalR;

namespace Dispatcher.Api.Realtime;

public sealed class SignalRTagValueBroadcaster : ITagValueBroadcaster
{
    private readonly IHubContext<TagValueHub> hubContext;

    public SignalRTagValueBroadcaster(IHubContext<TagValueHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public async Task BroadcastCurrentValueAsync(TagValueDto value, CancellationToken cancellationToken = default)
    {
        await hubContext.Clients
            .Group(RealtimeConstants.AllTagValuesGroupName)
            .SendAsync(RealtimeConstants.TagValueUpdatedEvent, value, cancellationToken);

        await hubContext.Clients
            .Group(RealtimeConstants.GetTagGroupName(value.TagId))
            .SendAsync(RealtimeConstants.TagValueUpdatedEvent, value, cancellationToken);
    }

    public Task BroadcastCurrentValuesSnapshotAsync(
        IReadOnlyCollection<TagValueDto> values,
        CancellationToken cancellationToken = default)
    {
        return hubContext.Clients
            .Group(RealtimeConstants.AllTagValuesGroupName)
            .SendAsync(RealtimeConstants.CurrentValuesSnapshotEvent, values, cancellationToken);
    }
}
