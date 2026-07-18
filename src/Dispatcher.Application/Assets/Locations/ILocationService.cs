using Dispatcher.Contracts.Assets;

namespace Dispatcher.Application.Assets.Locations;

public interface ILocationService
{
    Task<IReadOnlyList<LocationDto>> ListAsync(bool includeArchived, CancellationToken cancellationToken);

    Task<LocationDto?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<LocationDto> CreateAsync(CreateLocationRequest request, CancellationToken cancellationToken);

    Task<LocationDto?> UpdateAsync(Guid id, UpdateLocationRequest request, CancellationToken cancellationToken);

    Task<LocationDto?> MoveAsync(Guid id, MoveLocationRequest request, CancellationToken cancellationToken);

    Task<LocationDto?> ArchiveAsync(Guid id, ArchiveLocationRequest request, CancellationToken cancellationToken);
}
