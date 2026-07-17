namespace Dispatcher.Application.Tags;

public interface ITagValueService
{
    Task<TagValueDto?> GetCurrentValueAsync(Guid tagId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TagValueDto>> GetCurrentValuesAsync(CancellationToken cancellationToken = default);

    Task UpsertCurrentValueAsync(TagValueDto value, CancellationToken cancellationToken = default);
}
