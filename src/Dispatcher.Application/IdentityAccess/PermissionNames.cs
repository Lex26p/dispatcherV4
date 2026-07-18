namespace Dispatcher.Application.IdentityAccess;

public static class PermissionNames
{
    public const string Wildcard = "*";

    public const string IdentityMeView = "identity.me.view";
    public const string IdentityUsersView = "identity.users.view";
    public const string IdentityUsersManage = "identity.users.manage";
    public const string IdentityRolesView = "identity.roles.view";
    public const string IdentityRolesManage = "identity.roles.manage";
    public const string IdentityScopesView = "identity.scopes.view";
    public const string IdentityAssignmentsView = "identity.assignments.view";
    public const string IdentityAssignmentsManage = "identity.assignments.manage";

    public const string LocationsView = "locations.view";
    public const string LocationsManage = "locations.manage";

    public static IReadOnlyList<string> All { get; } =
    [
        IdentityMeView,
        IdentityUsersView,
        IdentityUsersManage,
        IdentityRolesView,
        IdentityRolesManage,
        IdentityScopesView,
        IdentityAssignmentsView,
        IdentityAssignmentsManage,
        LocationsView,
        LocationsManage,
    ];
}
