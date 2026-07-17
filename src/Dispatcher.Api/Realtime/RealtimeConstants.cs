namespace Dispatcher.Api.Realtime;

public static class RealtimeConstants
{
    public const string TagValuesHubPath = "/hubs/tag-values";

    public const string AllTagValuesGroupName = "tag-values:all";

    public const string TagValueUpdatedEvent = "TagValueUpdated";

    public const string CurrentValuesSnapshotEvent = "CurrentValuesSnapshot";

    public static string GetTagGroupName(Guid tagId)
    {
        return $"tag-values:tag:{tagId:N}";
    }
}
