using Dispatcher.Application.Assets.Locations;
using Dispatcher.Domain.Assets;
using Dispatcher.Domain.Common;
using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.Infrastructure.Assets;

public sealed class EfLocationRepository(DispatcherDbContext dbContext) : ILocationRepository
{
    public async Task<IReadOnlyList<Location>> ListAsync(bool includeArchived, CancellationToken cancellationToken)
    {
        var query = dbContext.Locations.AsQueryable();

        if (!includeArchived)
        {
            query = query.Where(location => !location.IsArchived);
        }

        return await query
            .OrderBy(location => location.Code)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Location?> GetAsync(EntityId id, CancellationToken cancellationToken)
    {
        return dbContext.Locations.SingleOrDefaultAsync(location => location.Id == id, cancellationToken);
    }

    public Task<Location?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return dbContext.Locations.SingleOrDefaultAsync(location => location.Code == normalizedCode, cancellationToken);
    }

    public Task<bool> ExistsAsync(EntityId id, CancellationToken cancellationToken)
    {
        return dbContext.Locations.AnyAsync(location => location.Id == id && !location.IsArchived, cancellationToken);
    }

    public Task<bool> HasActiveChildrenAsync(EntityId id, CancellationToken cancellationToken)
    {
        return dbContext.Locations.AnyAsync(
            location => location.ParentLocationId == id && !location.IsArchived,
            cancellationToken);
    }

    public void Add(Location location)
    {
        dbContext.Locations.Add(location);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
