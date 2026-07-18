using Dispatcher.Domain.Telemetry;
using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.IntegrationTests.Telemetry;

public sealed class TelemetryModelConfigurationTests
{
    [Fact]
    public void DispatcherDbContext_contains_telemetry_configuration_model()
    {
        var options = new DbContextOptionsBuilder<DispatcherDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Include Error Detail=false")
            .Options;

        using var dbContext = new DispatcherDbContext(options);
        var model = dbContext.Model;

        var source = model.FindEntityType(typeof(TelemetrySource));
        var point = model.FindEntityType(typeof(DataPoint));
        var mapping = model.FindEntityType(typeof(ProtocolMapping));
        var currentValue = model.FindEntityType(typeof(CurrentValue));
        var historicalValue = model.FindEntityType(typeof(HistoricalValue));

        Assert.NotNull(source);
        Assert.NotNull(point);
        Assert.NotNull(mapping);
        Assert.Equal(SchemaNames.Telemetry, source!.GetSchema());
        Assert.Equal("telemetry_sources", source.GetTableName());
        Assert.Equal(SchemaNames.Telemetry, point!.GetSchema());
        Assert.Equal("data_points", point.GetTableName());
        Assert.Equal(SchemaNames.Telemetry, mapping!.GetSchema());
        Assert.Equal("protocol_mappings", mapping.GetTableName());
        Assert.NotNull(currentValue);
        Assert.NotNull(historicalValue);
        Assert.Equal(SchemaNames.Telemetry, currentValue!.GetSchema());
        Assert.Equal("current_values", currentValue.GetTableName());
        Assert.Equal(SchemaNames.Telemetry, historicalValue!.GetSchema());
        Assert.Equal("historical_values", historicalValue.GetTableName());
    }
}
