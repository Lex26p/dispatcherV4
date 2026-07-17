using Dispatcher.Domain.Tags;

namespace Dispatcher.Application.Abstractions.Persistence;

public interface ITagValueRepository
{
    Task<TagValue?> GetCurrentValueAsync(Guid tagId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TagValue>> GetCurrentValuesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TagValue>> GetCurrentValuesAsync(IEnumerable<Guid> tagIds, CancellationToken cancellationToken = default);

    Task UpsertCurrentValueAsync(TagValue value, CancellationToken cancellationToken = default);
}
