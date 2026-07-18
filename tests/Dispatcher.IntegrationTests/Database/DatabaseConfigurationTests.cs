using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.IntegrationTests.Database;

public sealed class DatabaseConfigurationTests
{
    [Fact]
    public void DispatcherDbContext_uses_npgsql_provider()
    {
        var options = new DbContextOptionsBuilder<DispatcherDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Include Error Detail=false")
            .Options;

        using var dbContext = new DispatcherDbContext(options);

        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", dbContext.Database.ProviderName);
    }

    [Fact]
    public async Task DispatcherDbContext_can_connect_when_database_tests_are_enabled()
    {
        if (!DatabaseFixture.ShouldRunDatabaseTests)
        {
            return;
        }

        var options = new DbContextOptionsBuilder<DispatcherDbContext>()
            .UseNpgsql(DatabaseFixture.ConnectionString!)
            .Options;

        await using var dbContext = new DispatcherDbContext(options);

        Assert.True(await dbContext.Database.CanConnectAsync());
    }
}
