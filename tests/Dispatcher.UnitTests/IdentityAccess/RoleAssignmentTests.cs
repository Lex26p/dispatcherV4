using Dispatcher.Domain.Common;
using Dispatcher.Domain.IdentityAccess;

namespace Dispatcher.UnitTests.IdentityAccess;

public sealed class RoleAssignmentTests
{
    [Fact]
    public void Revoke_requires_reason_and_marks_assignment_inactive()
    {
        var assignment = RoleAssignment.Grant(
            EntityId.New(),
            EntityId.New(),
            EntityId.New(),
            EntityId.New(),
            "manual",
            "grant for test",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(() => assignment.Revoke(DateTimeOffset.UtcNow, " "));

        assignment.Revoke(DateTimeOffset.UtcNow, "no longer needed");

        Assert.False(assignment.IsActive);
        Assert.Equal("no longer needed", assignment.RevokedReason);
    }
}
