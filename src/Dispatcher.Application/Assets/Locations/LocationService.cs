using Dispatcher.Application.Abstractions;
using Dispatcher.Contracts.Assets;
using Dispatcher.Domain.Assets;
using Dispatcher.Domain.Common;

namespace Dispatcher.Application.Assets.Locations;

public sealed class LocationService(ILocationRepository locations, IClock clock) : ILocationService
{
    public async Task<IReadOnlyList<LocationDto>> ListAsync(bool includeArchived, CancellationToken cancellationToken)
    {
        var items = await locations.ListAsync(includeArchived, cancellationToken);
        return items
            .OrderBy(location => location.Code, StringComparer.OrdinalIgnoreCase)
            .Select(ToDto)
            .ToArray();
    }

    public async Task<LocationDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var location = await locations.GetAsync(EntityId.From(id), cancellationToken);
        return location is null ? null : ToDto(location);
    }

    public async Task<LocationDto> CreateAsync(CreateLocationRequest request, CancellationToken cancellationToken)
    {
        EntityId? parentLocationId = request.ParentLocationId.HasValue
            ? EntityId.From(request.ParentLocationId.Value)
            : null;

        if (parentLocationId.HasValue && !await locations.ExistsAsync(parentLocationId.Value, cancellationToken))
        {
            throw new InvalidOperationException("Parent location was not found.");
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var existing = await locations.GetByCodeAsync(normalizedCode, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("Location code already exists.");
        }

        var location = Location.Create(
            EntityId.New(),
            parentLocationId,
            request.Code,
            request.Name,
            request.Description,
            clock.UtcNow);

        locations.Add(location);
        await locations.SaveChangesAsync(cancellationToken);

        return ToDto(location);
    }

    public async Task<LocationDto?> UpdateAsync(Guid id, UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        var location = await locations.GetAsync(EntityId.From(id), cancellationToken);
        if (location is null)
        {
            return null;
        }

        location.Rename(request.Name, request.Description, clock.UtcNow);
        await locations.SaveChangesAsync(cancellationToken);

        return ToDto(location);
    }

    public async Task<LocationDto?> MoveAsync(Guid id, MoveLocationRequest request, CancellationToken cancellationToken)
    {
        var locationId = EntityId.From(id);
        var location = await locations.GetAsync(locationId, cancellationToken);
        if (location is null)
        {
            return null;
        }

        EntityId? parentLocationId = request.ParentLocationId.HasValue
            ? EntityId.From(request.ParentLocationId.Value)
            : null;

        if (parentLocationId.HasValue)
        {
            if (parentLocationId.Value == locationId)
            {
                throw new InvalidOperationException("Location cannot be its own parent.");
            }

            var allLocations = await locations.ListAsync(includeArchived: true, cancellationToken);
            var parentLookup = allLocations.ToDictionary(item => item.Id, item => item.ParentLocationId);

            if (!parentLookup.ContainsKey(parentLocationId.Value))
            {
                throw new InvalidOperationException("Parent location was not found.");
            }

            var currentParent = parentLocationId;
            while (currentParent.HasValue)
            {
                if (currentParent.Value == locationId)
                {
                    throw new InvalidOperationException("Location hierarchy cannot contain cycles.");
                }

                currentParent = parentLookup.TryGetValue(currentParent.Value, out var nextParent)
                    ? nextParent
                    : null;
            }
        }

        location.MoveTo(parentLocationId, clock.UtcNow);
        await locations.SaveChangesAsync(cancellationToken);

        return ToDto(location);
    }

    public async Task<LocationDto?> ArchiveAsync(Guid id, ArchiveLocationRequest request, CancellationToken cancellationToken)
    {
        var locationId = EntityId.From(id);
        var location = await locations.GetAsync(locationId, cancellationToken);
        if (location is null)
        {
            return null;
        }

        var hasActiveChildren = await locations.HasActiveChildrenAsync(locationId, cancellationToken);
        if (hasActiveChildren)
        {
            throw new InvalidOperationException("Location with active children cannot be archived.");
        }

        location.Archive(clock.UtcNow);
        await locations.SaveChangesAsync(cancellationToken);

        return ToDto(location);
    }

    private static LocationDto ToDto(Location location) => new(
        location.Id.Value,
        location.ParentLocationId?.Value,
        location.Code,
        location.Name,
        location.Description,
        location.IsArchived,
        location.CreatedAtUtc,
        location.UpdatedAtUtc);
}
