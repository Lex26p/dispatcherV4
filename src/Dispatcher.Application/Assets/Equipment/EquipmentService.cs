using Dispatcher.Application.Abstractions;
using Dispatcher.Contracts.Assets;
using Dispatcher.Domain.Common;
using AssetEquipment = Dispatcher.Domain.Assets.Equipment;

namespace Dispatcher.Application.Assets.Equipment;

public sealed class EquipmentService(IEquipmentRepository equipmentRepository, IClock clock) : IEquipmentService
{
    public async Task<IReadOnlyList<EquipmentDto>> ListAsync(Guid? locationId, bool includeArchived, CancellationToken cancellationToken)
    {
        EntityId? typedLocationId = locationId.HasValue
            ? EntityId.From(locationId.Value)
            : null;

        var items = await equipmentRepository.ListAsync(typedLocationId, includeArchived, cancellationToken);

        return items
            .OrderBy(item => item.Code, StringComparer.OrdinalIgnoreCase)
            .Select(ToDto)
            .ToArray();
    }

    public async Task<EquipmentDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var equipment = await equipmentRepository.GetAsync(EntityId.From(id), cancellationToken);
        return equipment is null ? null : ToDto(equipment);
    }

    public async Task<EquipmentDto> CreateAsync(CreateEquipmentRequest request, CancellationToken cancellationToken)
    {
        var locationId = EntityId.From(request.LocationId);
        await EnsureLocationExistsAsync(locationId, cancellationToken);

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var existing = await equipmentRepository.GetByCodeAsync(normalizedCode, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("Equipment code already exists.");
        }

        var equipment = AssetEquipment.Create(
            EntityId.New(),
            locationId,
            request.Code,
            request.Name,
            request.Description,
            clock.UtcNow);

        equipmentRepository.Add(equipment);
        await equipmentRepository.SaveChangesAsync(cancellationToken);

        return ToDto(equipment);
    }

    public async Task<EquipmentDto?> UpdateAsync(Guid id, UpdateEquipmentRequest request, CancellationToken cancellationToken)
    {
        var equipment = await equipmentRepository.GetAsync(EntityId.From(id), cancellationToken);
        if (equipment is null)
        {
            return null;
        }

        equipment.Rename(request.Name, request.Description, clock.UtcNow);
        await equipmentRepository.SaveChangesAsync(cancellationToken);

        return ToDto(equipment);
    }

    public async Task<EquipmentDto?> MoveAsync(Guid id, MoveEquipmentRequest request, CancellationToken cancellationToken)
    {
        var equipment = await equipmentRepository.GetAsync(EntityId.From(id), cancellationToken);
        if (equipment is null)
        {
            return null;
        }

        var locationId = EntityId.From(request.LocationId);
        await EnsureLocationExistsAsync(locationId, cancellationToken);

        equipment.MoveToLocation(locationId, clock.UtcNow);
        await equipmentRepository.SaveChangesAsync(cancellationToken);

        return ToDto(equipment);
    }

    public async Task<EquipmentDto?> ArchiveAsync(Guid id, ArchiveEquipmentRequest request, CancellationToken cancellationToken)
    {
        var equipment = await equipmentRepository.GetAsync(EntityId.From(id), cancellationToken);
        if (equipment is null)
        {
            return null;
        }

        equipment.Archive(clock.UtcNow);
        await equipmentRepository.SaveChangesAsync(cancellationToken);

        return ToDto(equipment);
    }

    public async Task<EquipmentDto?> RestoreAsync(Guid id, RestoreEquipmentRequest request, CancellationToken cancellationToken)
    {
        var equipment = await equipmentRepository.GetAsync(EntityId.From(id), cancellationToken);
        if (equipment is null)
        {
            return null;
        }

        await EnsureLocationExistsAsync(equipment.LocationId, cancellationToken);

        equipment.Restore(clock.UtcNow);
        await equipmentRepository.SaveChangesAsync(cancellationToken);

        return ToDto(equipment);
    }

    private async Task EnsureLocationExistsAsync(EntityId locationId, CancellationToken cancellationToken)
    {
        var exists = await equipmentRepository.LocationExistsAsync(locationId, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException("Location was not found or is archived.");
        }
    }

    private static EquipmentDto ToDto(AssetEquipment equipment) => new(
        equipment.Id.Value,
        equipment.LocationId.Value,
        equipment.Code,
        equipment.Name,
        equipment.Description,
        equipment.IsArchived,
        equipment.CreatedAtUtc,
        equipment.UpdatedAtUtc);
}
