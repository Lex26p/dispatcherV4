using Dispatcher.Domain.Tags;

namespace Dispatcher.Application.Abstractions.Persistence;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Tag?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Tag>> GetByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Tag>> GetEnabledAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Tag tag, CancellationToken cancellationToken = default);

    void Update(Tag tag);

    void Remove(Tag tag);
}
