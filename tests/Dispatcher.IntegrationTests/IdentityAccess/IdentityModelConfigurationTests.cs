using Dispatcher.Domain.Common;
using Dispatcher.Domain.IdentityAccess;
using Dispatcher.Infrastructure.Persistence;
using Dispatcher.IntegrationTests.Database;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.IntegrationTests.IdentityAccess;

public sealed class IdentityModelConfigurationTests
{
    [Fact]
    public void DispatcherDbContext_contains_identity_access_model()
    {
        var options = new DbContextOptionsBuilder<DispatcherDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Include Error Detail=false")
            .Options;

        using var dbContext = new DispatcherDbContext(options);
        var model = dbContext.Model;

        Assert.NotNull(model.FindEntityType(typeof(UserAccount)));
        Assert.NotNull(model.FindEntityType(typeof(Role)));
        Assert.NotNull(model.FindEntityType(typeof(PermissionScope)));
        Assert.NotNull(model.FindEntityType(typeof(RoleAssignment)));
    }

    [Fact]
    public async Task Seeded_development_admin_exists_when_database_tests_are_enabled()
    {
        if (!DatabaseFixture.ShouldRunDatabaseTests)
        {
            return;
        }

        var options = new DbContextOptionsBuilder<DispatcherDbContext>()
            .UseNpgsql(DatabaseFixture.ConnectionString!)
            .Options;

        await using var dbContext = new DispatcherDbContext(options);
        var adminId = EntityId.From(Guid.Parse("10000000-0000-0000-0000-000000000001"));

        Assert.True(await dbContext.UserAccounts.AnyAsync(user => user.Id == adminId));
    }
}
