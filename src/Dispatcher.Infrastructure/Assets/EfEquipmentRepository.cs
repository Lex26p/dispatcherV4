using Dispatcher.Application.Assets.Equipment;
using Dispatcher.Domain.Assets;
using Dispatcher.Domain.Common;
using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.Infrastructure.Assets;

public sealed class EfEquipmentRepository(DispatcherDbContext dbContext) : IEquipmentRepository
{
    public async Task<IReadOnlyList<Equipment>> ListAsync(EntityId? locationId, bool includeArchived, CancellationToken cancellationToken)
    {
        var query = dbContext.Equipment.AsQueryable();

        if (locationId.HasValue)
        {
            query = query.Where(equipment => equipment.LocationId == locationId.Value);
        }

        if (!includeArchived)
        {
            query = query.Where(equipment => !equipment.IsArchived);
        }

        return await query
            .OrderBy(equipment => equipment.Code)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Equipment?> GetAsync(EntityId id, CancellationToken cancellationToken)
    {
        return dbContext.Equipment.SingleOrDefaultAsync(equipment => equipment.Id == id, cancellationToken);
    }

    public Task<Equipment?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return dbContext.Equipment.SingleOrDefaultAsync(equipment => equipment.Code == normalizedCode, cancellationToken);
    }

    public Task<bool> LocationExistsAsync(EntityId locationId, CancellationToken cancellationToken)
    {
        return dbContext.Locations.AnyAsync(location => location.Id == locationId && !location.IsArchived, cancellationToken);
    }

    public void Add(Equipment equipment)
    {
        dbContext.Equipment.Add(equipment);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
