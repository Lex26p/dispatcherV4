using Dispatcher.Application.Abstractions.Persistence;
using Dispatcher.Domain.Tags;

namespace Dispatcher.Application.Tags;

internal sealed class TagValueService : ITagValueService
{
    private readonly ITagRepository _tagRepository;
    private readonly ITagValueRepository _tagValueRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TagValueService(
        ITagRepository tagRepository,
        ITagValueRepository tagValueRepository,
        IUnitOfWork unitOfWork)
    {
        _tagRepository = tagRepository;
        _tagValueRepository = tagValueRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TagValueDto?> GetCurrentValueAsync(
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        var value = await _tagValueRepository.GetCurrentValueAsync(tagId, cancellationToken);
        return value?.ToDto();
    }

    public async Task<IReadOnlyList<TagValueDto>> GetCurrentValuesAsync(
        CancellationToken cancellationToken = default)
    {
        var values = await _tagValueRepository.GetCurrentValuesAsync(cancellationToken);
        return values.Select(value => value.ToDto()).ToArray();
    }

    public async Task UpsertCurrentValueAsync(
        TagValueDto value,
        CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(value.TagId, cancellationToken);

        if (tag is null)
        {
            throw new InvalidOperationException($"Tag with id '{value.TagId}' was not found.");
        }

        var currentValue = new TagValue(
            value.TagId,
            value.DeviceId,
            value.Value,
            value.Quality,
            value.Timestamp,
            value.ErrorMessage);

        await _tagValueRepository.UpsertCurrentValueAsync(currentValue, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
