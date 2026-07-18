using Dispatcher.Contracts.Assets;

namespace Dispatcher.Application.Assets.Equipment;

public interface IEquipmentService
{
    Task<IReadOnlyList<EquipmentDto>> ListAsync(Guid? locationId, bool includeArchived, CancellationToken cancellationToken);

    Task<EquipmentDto?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<EquipmentDto> CreateAsync(CreateEquipmentRequest request, CancellationToken cancellationToken);

    Task<EquipmentDto?> UpdateAsync(Guid id, UpdateEquipmentRequest request, CancellationToken cancellationToken);

    Task<EquipmentDto?> MoveAsync(Guid id, MoveEquipmentRequest request, CancellationToken cancellationToken);

    Task<EquipmentDto?> ArchiveAsync(Guid id, ArchiveEquipmentRequest request, CancellationToken cancellationToken);

    Task<EquipmentDto?> RestoreAsync(Guid id, RestoreEquipmentRequest request, CancellationToken cancellationToken);
}
