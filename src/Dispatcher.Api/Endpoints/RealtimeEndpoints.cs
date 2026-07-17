using Dispatcher.Api.Realtime;

namespace Dispatcher.Api.Endpoints;

public static class RealtimeEndpoints
{
    public static IEndpointRouteBuilder MapRealtimeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/realtime", () => Results.Ok(new
        {
            tagValuesHub = RealtimeConstants.TagValuesHubPath,
            groups = new
            {
                allTagValues = RealtimeConstants.AllTagValuesGroupName,
                tagSpecificFormat = "tag-values:tag:{tagId}"
            },
            hubMethods = new
            {
                subscribeToAll = "SubscribeToAll",
                unsubscribeFromAll = "UnsubscribeFromAll",
                subscribeToTag = "SubscribeToTag",
                unsubscribeFromTag = "UnsubscribeFromTag"
            },
            clientEvents = new
            {
                tagValueUpdated = RealtimeConstants.TagValueUpdatedEvent,
                currentValuesSnapshot = RealtimeConstants.CurrentValuesSnapshotEvent
            }
        }))
        .WithTags("Realtime")
        .WithName("GetRealtimeInfo");

        return app;
    }
}
