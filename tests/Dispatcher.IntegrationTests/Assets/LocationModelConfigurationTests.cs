using Dispatcher.Domain.Assets;
using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.IntegrationTests.Assets;

public sealed class LocationModelConfigurationTests
{
    [Fact]
    public void DispatcherDbContext_contains_assets_location_model()
    {
        var options = new DbContextOptionsBuilder<DispatcherDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Include Error Detail=false")
            .Options;

        using var dbContext = new DispatcherDbContext(options);
        var entityType = dbContext.Model.FindEntityType(typeof(Location));

        Assert.NotNull(entityType);
        Assert.Equal(SchemaNames.Assets, entityType!.GetSchema());
        Assert.Equal("locations", entityType.GetTableName());
    }
}
