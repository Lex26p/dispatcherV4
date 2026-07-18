using Dispatcher.Application.IdentityAccess;

namespace Dispatcher.Infrastructure.Identity;

public static class DevelopmentIdentitySeed
{
    public static readonly Guid AdminUserId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid AdministratorRoleId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid EngineerRoleId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid OperatorRoleId = Guid.Parse("20000000-0000-0000-0000-000000000003");
    public static readonly Guid GlobalScopeId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    public static readonly Guid AdminAssignmentId = Guid.Parse("40000000-0000-0000-0000-000000000001");

    public const string AdminExternalId = "dev-admin";
    public const string AdminDisplayName = "Development Administrator";
    public const string AdminEmail = "dev-admin@dispatcher.local";

    public static string AdministratorPermissionsText => PermissionNames.Wildcard;

    public static string EngineerPermissionsText => string.Join(';',
    [
        PermissionNames.IdentityMeView,
        PermissionNames.IdentityUsersView,
        PermissionNames.IdentityRolesView,
        PermissionNames.IdentityScopesView,
        PermissionNames.IdentityAssignmentsView,
    ]);

    public static string OperatorPermissionsText => PermissionNames.IdentityMeView;
}
