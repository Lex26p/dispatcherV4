using Dispatcher.Application.Abstractions;
using Dispatcher.Application.Assets.Equipment;
using Dispatcher.Contracts.Assets;
using Dispatcher.Domain.Common;
using AssetEquipment = Dispatcher.Domain.Assets.Equipment;

namespace Dispatcher.UnitTests.Assets;

public sealed class EquipmentServiceTests
{
    [Fact]
    public async Task CreateAsync_creates_equipment_without_protocol_fields()
    {
        var clock = new FixedClock();
        var locationId = EntityId.New();
        var repository = new InMemoryEquipmentRepository();
        repository.ActiveLocations.Add(locationId);
        var service = new EquipmentService(repository, clock);

        var created = await service.CreateAsync(
            new CreateEquipmentRequest(locationId.Value, "pump-1", "Pump 1", "Main pump"),
            CancellationToken.None);

        Assert.Equal(locationId.Value, created.LocationId);
        Assert.Equal("PUMP-1", created.Code);
        Assert.False(created.IsArchived);
        Assert.Single(repository.Items);
    }

    [Fact]
    public async Task CreateAsync_rejects_missing_location()
    {
        var service = new EquipmentService(new InMemoryEquipmentRepository(), new FixedClock());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateEquipmentRequest(Guid.NewGuid(), "PUMP-1", "Pump", null), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_rejects_duplicate_code()
    {
        var clock = new FixedClock();
        var locationId = EntityId.New();
        var repository = new InMemoryEquipmentRepository();
        repository.ActiveLocations.Add(locationId);
        repository.Items.Add(AssetEquipment.Create(EntityId.New(), locationId, "PUMP-1", "Pump", null, clock.UtcNow));
        var service = new EquipmentService(repository, clock);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateEquipmentRequest(locationId.Value, "pump-1", "Other pump", null), CancellationToken.None));
    }

    [Fact]
    public async Task ArchiveAsync_marks_equipment_archived_without_deleting_it()
    {
        var clock = new FixedClock();
        var locationId = EntityId.New();
        var equipment = AssetEquipment.Create(EntityId.New(), locationId, "PUMP-1", "Pump", null, clock.UtcNow);
        var repository = new InMemoryEquipmentRepository();
        repository.ActiveLocations.Add(locationId);
        repository.Items.Add(equipment);
        var service = new EquipmentService(repository, clock);

        var archived = await service.ArchiveAsync(equipment.Id.Value, new ArchiveEquipmentRequest("test"), CancellationToken.None);

        Assert.NotNull(archived);
        Assert.True(archived.IsArchived);
        Assert.Single(repository.Items);
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 7, 18, 0, 0, 0, TimeSpan.Zero);
    }

    private sealed class InMemoryEquipmentRepository : IEquipmentRepository
    {
        public List<AssetEquipment> Items { get; } = [];

        public HashSet<EntityId> ActiveLocations { get; } = [];

        public Task<IReadOnlyList<AssetEquipment>> ListAsync(EntityId? locationId, bool includeArchived, CancellationToken cancellationToken)
        {
            IReadOnlyList<AssetEquipment> items = Items
                .Where(equipment => !locationId.HasValue || equipment.LocationId == locationId.Value)
                .Where(equipment => includeArchived || !equipment.IsArchived)
                .ToArray();

            return Task.FromResult(items);
        }

        public Task<AssetEquipment?> GetAsync(EntityId id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.SingleOrDefault(equipment => equipment.Id == id));
        }

        public Task<AssetEquipment?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        {
            var normalizedCode = code.Trim().ToUpperInvariant();
            return Task.FromResult(Items.SingleOrDefault(equipment => equipment.Code == normalizedCode));
        }

        public Task<bool> LocationExistsAsync(EntityId locationId, CancellationToken cancellationToken)
        {
            return Task.FromResult(ActiveLocations.Contains(locationId));
        }

        public void Add(AssetEquipment equipment)
        {
            Items.Add(equipment);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
