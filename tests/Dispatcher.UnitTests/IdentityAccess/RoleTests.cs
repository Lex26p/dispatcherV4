using Dispatcher.Application.IdentityAccess;
using Dispatcher.Domain.Common;
using Dispatcher.Domain.IdentityAccess;

namespace Dispatcher.UnitTests.IdentityAccess;

public sealed class RoleTests
{
    [Fact]
    public void Role_normalizes_permissions_and_supports_wildcard()
    {
        var role = Role.Create(
            EntityId.New(),
            "Administrator",
            "Administrator",
            null,
            [PermissionNames.IdentityUsersView, PermissionNames.IdentityUsersView, PermissionNames.Wildcard],
            isSystem: true);

        Assert.Equal("administrator", role.Code);
        Assert.True(role.HasPermission(PermissionNames.IdentityAssignmentsManage));
        Assert.Contains(PermissionNames.Wildcard, role.PermissionSet);
    }

    [Fact]
    public void Role_requires_at_least_one_permission()
    {
        Assert.Throws<ArgumentException>(() => Role.Create(EntityId.New(), "empty", "Empty", null, [], isSystem: false));
    }
}
