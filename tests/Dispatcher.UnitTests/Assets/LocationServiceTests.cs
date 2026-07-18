using Dispatcher.Application.Abstractions;
using Dispatcher.Application.Assets.Locations;
using Dispatcher.Contracts.Assets;
using Dispatcher.Domain.Assets;
using Dispatcher.Domain.Common;

namespace Dispatcher.UnitTests.Assets;

public sealed class LocationServiceTests
{
    [Fact]
    public async Task CreateAsync_creates_root_location()
    {
        var repository = new InMemoryLocationRepository();
        var service = new LocationService(repository, new FixedClock());

        var created = await service.CreateAsync(
            new CreateLocationRequest(null, "site-1", "Main site", "Root location"),
            CancellationToken.None);

        Assert.Equal("SITE-1", created.Code);
        Assert.Null(created.ParentLocationId);
        Assert.False(created.IsArchived);
        Assert.Single(repository.Items);
    }

    [Fact]
    public async Task MoveAsync_rejects_hierarchy_cycle()
    {
        var clock = new FixedClock();
        var repository = new InMemoryLocationRepository();
        var root = Location.Create(EntityId.New(), null, "ROOT", "Root", null, clock.UtcNow);
        var child = Location.Create(EntityId.New(), root.Id, "CHILD", "Child", null, clock.UtcNow);
        repository.Items.Add(root);
        repository.Items.Add(child);
        var service = new LocationService(repository, clock);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.MoveAsync(root.Id.Value, new MoveLocationRequest(child.Id.Value), CancellationToken.None));
    }

    [Fact]
    public async Task ArchiveAsync_rejects_location_with_active_children()
    {
        var clock = new FixedClock();
        var repository = new InMemoryLocationRepository();
        var root = Location.Create(EntityId.New(), null, "ROOT", "Root", null, clock.UtcNow);
        var child = Location.Create(EntityId.New(), root.Id, "CHILD", "Child", null, clock.UtcNow);
        repository.Items.Add(root);
        repository.Items.Add(child);
        var service = new LocationService(repository, clock);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ArchiveAsync(root.Id.Value, new ArchiveLocationRequest("test"), CancellationToken.None));
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 7, 18, 0, 0, 0, TimeSpan.Zero);
    }

    private sealed class InMemoryLocationRepository : ILocationRepository
    {
        public List<Location> Items { get; } = [];

        public Task<IReadOnlyList<Location>> ListAsync(bool includeArchived, CancellationToken cancellationToken)
        {
            IReadOnlyList<Location> items = Items
                .Where(location => includeArchived || !location.IsArchived)
                .ToArray();

            return Task.FromResult(items);
        }

        public Task<Location?> GetAsync(EntityId id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.SingleOrDefault(location => location.Id == id));
        }

        public Task<Location?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        {
            var normalizedCode = code.Trim().ToUpperInvariant();
            return Task.FromResult(Items.SingleOrDefault(location => location.Code == normalizedCode));
        }

        public Task<bool> ExistsAsync(EntityId id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.Any(location => location.Id == id && !location.IsArchived));
        }

        public Task<bool> HasActiveChildrenAsync(EntityId id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.Any(location => location.ParentLocationId == id && !location.IsArchived));
        }

        public void Add(Location location)
        {
            Items.Add(location);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
