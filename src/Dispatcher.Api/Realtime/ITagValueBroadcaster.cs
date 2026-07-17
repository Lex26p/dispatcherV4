using Dispatcher.Application.Tags;

namespace Dispatcher.Api.Realtime;

public interface ITagValueBroadcaster
{
    Task BroadcastCurrentValueAsync(TagValueDto value, CancellationToken cancellationToken = default);

    Task BroadcastCurrentValuesSnapshotAsync(IReadOnlyCollection<TagValueDto> values, CancellationToken cancellationToken = default);
}
