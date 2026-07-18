using Dispatcher.Domain.Assets;
using Dispatcher.Domain.Common;

namespace Dispatcher.Application.Assets.Locations;

public interface ILocationRepository
{
    Task<IReadOnlyList<Location>> ListAsync(bool includeArchived, CancellationToken cancellationToken);

    Task<Location?> GetAsync(EntityId id, CancellationToken cancellationToken);

    Task<Location?> GetByCodeAsync(string code, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(EntityId id, CancellationToken cancellationToken);

    Task<bool> HasActiveChildrenAsync(EntityId id, CancellationToken cancellationToken);

    void Add(Location location);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
