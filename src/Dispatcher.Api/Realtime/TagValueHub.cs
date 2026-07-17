using Microsoft.AspNetCore.SignalR;

namespace Dispatcher.Api.Realtime;

public sealed class TagValueHub : Hub
{
    public Task SubscribeToAll()
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, RealtimeConstants.AllTagValuesGroupName);
    }

    public Task UnsubscribeFromAll()
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, RealtimeConstants.AllTagValuesGroupName);
    }

    public Task SubscribeToTag(Guid tagId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, RealtimeConstants.GetTagGroupName(tagId));
    }

    public Task UnsubscribeFromTag(Guid tagId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, RealtimeConstants.GetTagGroupName(tagId));
    }
}
