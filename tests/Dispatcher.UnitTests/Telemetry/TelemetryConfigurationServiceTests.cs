using Dispatcher.Application.Abstractions;
using Dispatcher.Application.Telemetry.Configuration;
using Dispatcher.Contracts.Telemetry;
using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;

namespace Dispatcher.UnitTests.Telemetry;

public sealed class TelemetryConfigurationServiceTests
{
    [Fact]
    public async Task CreateSourceAsync_ReturnsMaskedSecretReference()
    {
        var repository = new FakeTelemetryConfigurationRepository();
        var service = new TelemetryConfigurationService(repository, new FixedClock());

        var result = await service.CreateSourceAsync(
            new CreateTelemetrySourceRequest(
                "sim-01",
                "Simulator 01",
                "Simulator",
                "simulator://local",
                1,
                "{}",
                "secret://telemetry/sim-01",
                "test"),
            CancellationToken.None);

        Assert.True(result.HasSecretReference);
        Assert.Equal("secret://***", result.MaskedSecretReference);
        Assert.DoesNotContain("sim-01", result.MaskedSecretReference, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateDataPointAsync_RequiresActiveEquipment()
    {
        var repository = new FakeTelemetryConfigurationRepository();
        var service = new TelemetryConfigurationService(repository, new FixedClock());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateDataPointAsync(
            new CreateDataPointRequest(
                Guid.NewGuid(),
                "TEMP-001",
                "Temperature",
                "Decimal",
                "°C",
                60,
                null),
            CancellationToken.None));
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 7, 18, 0, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeTelemetryConfigurationRepository : ITelemetryConfigurationRepository
    {
        private readonly List<TelemetrySource> sources = [];
        private readonly List<DataPoint> points = [];
        private readonly List<ProtocolMapping> mappings = [];
        private readonly HashSet<EntityId> equipment = [];

        public Task<IReadOnlyList<TelemetrySource>> ListSourcesAsync(bool includeArchived, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<TelemetrySource>>(sources.Where(source => includeArchived || !source.IsArchived).ToArray());
        }

        public Task<TelemetrySource?> GetSourceAsync(EntityId id, CancellationToken cancellationToken)
        {
            return Task.FromResult(sources.SingleOrDefault(source => source.Id == id));
        }

        public Task<TelemetrySource?> GetSourceByCodeAsync(string code, CancellationToken cancellationToken)
        {
            return Task.FromResult(sources.SingleOrDefault(source => source.Code == code.Trim().ToUpperInvariant()));
        }

        public void AddSource(TelemetrySource source)
        {
            sources.Add(source);
        }

        public Task<IReadOnlyList<DataPoint>> ListDataPointsAsync(EntityId? equipmentId, bool includeArchived, CancellationToken cancellationToken)
        {
            var query = points.AsEnumerable();
            if (equipmentId.HasValue)
            {
                query = query.Where(point => point.EquipmentId == equipmentId.Value);
            }

            if (!includeArchived)
            {
                query = query.Where(point => !point.IsArchived);
            }

            return Task.FromResult<IReadOnlyList<DataPoint>>(query.ToArray());
        }

        public Task<DataPoint?> GetDataPointAsync(EntityId id, CancellationToken cancellationToken)
        {
            return Task.FromResult(points.SingleOrDefault(point => point.Id == id));
        }

        public Task<DataPoint?> GetDataPointByCodeAsync(string code, CancellationToken cancellationToken)
        {
            return Task.FromResult(points.SingleOrDefault(point => point.Code == code.Trim().ToUpperInvariant()));
        }

        public void AddDataPoint(DataPoint dataPoint)
        {
            points.Add(dataPoint);
        }

        public Task<IReadOnlyList<ProtocolMapping>> ListMappingsAsync(EntityId? dataPointId, EntityId? telemetrySourceId, bool includeArchived, CancellationToken cancellationToken)
        {
            var query = mappings.AsEnumerable();
            if (dataPointId.HasValue)
            {
                query = query.Where(mapping => mapping.DataPointId == dataPointId.Value);
            }

            if (telemetrySourceId.HasValue)
            {
                query = query.Where(mapping => mapping.TelemetrySourceId == telemetrySourceId.Value);
            }

            if (!includeArchived)
            {
                query = query.Where(mapping => !mapping.IsArchived);
            }

            return Task.FromResult<IReadOnlyList<ProtocolMapping>>(query.ToArray());
        }

        public Task<ProtocolMapping?> GetMappingAsync(EntityId id, CancellationToken cancellationToken)
        {
            return Task.FromResult(mappings.SingleOrDefault(mapping => mapping.Id == id));
        }

        public Task<ProtocolMapping?> GetMappingByDataPointAsync(EntityId dataPointId, CancellationToken cancellationToken)
        {
            return Task.FromResult(mappings.SingleOrDefault(mapping => mapping.DataPointId == dataPointId && !mapping.IsArchived));
        }

        public void AddMapping(ProtocolMapping mapping)
        {
            mappings.Add(mapping);
        }

        public Task<bool> EquipmentExistsAsync(EntityId equipmentId, CancellationToken cancellationToken)
        {
            return Task.FromResult(equipment.Contains(equipmentId));
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
