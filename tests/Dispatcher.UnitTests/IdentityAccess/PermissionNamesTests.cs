using Dispatcher.Application.IdentityAccess;

namespace Dispatcher.UnitTests.IdentityAccess;

public sealed class PermissionNamesTests
{
    [Fact]
    public void Permission_names_are_unique_and_machine_readable()
    {
        Assert.Equal(PermissionNames.All.Count, PermissionNames.All.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(PermissionNames.All, permission => Assert.Matches("^[a-z0-9_.-]+$", permission));
    }
}
