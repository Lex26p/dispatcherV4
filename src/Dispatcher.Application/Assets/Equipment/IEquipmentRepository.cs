using Dispatcher.Domain.Common;
using AssetEquipment = Dispatcher.Domain.Assets.Equipment;

namespace Dispatcher.Application.Assets.Equipment;

public interface IEquipmentRepository
{
    Task<IReadOnlyList<AssetEquipment>> ListAsync(EntityId? locationId, bool includeArchived, CancellationToken cancellationToken);

    Task<AssetEquipment?> GetAsync(EntityId id, CancellationToken cancellationToken);

    Task<AssetEquipment?> GetByCodeAsync(string code, CancellationToken cancellationToken);

    Task<bool> LocationExistsAsync(EntityId locationId, CancellationToken cancellationToken);

    void Add(AssetEquipment equipment);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
