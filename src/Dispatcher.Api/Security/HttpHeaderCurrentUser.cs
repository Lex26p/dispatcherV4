using Dispatcher.Application.Abstractions;
using Dispatcher.Application.IdentityAccess;

namespace Dispatcher.Api.Security;

public sealed class HttpHeaderCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private const string UserIdHeader = "X-Dispatcher-User-Id";
    private const string UserNameHeader = "X-Dispatcher-User-Name";
    private const string PermissionsHeader = "X-Dispatcher-Permissions";

    public bool IsAuthenticated => true;

    public string? UserId => ReadHeader(UserIdHeader) ?? "dev-admin";

    public string? DisplayName => ReadHeader(UserNameHeader) ?? "Development Administrator";

    public IReadOnlySet<string> Permissions
    {
        get
        {
            var explicitPermissions = ReadHeader(PermissionsHeader);
            if (!string.IsNullOrWhiteSpace(explicitPermissions))
            {
                return explicitPermissions
                    .Split(',', ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            return PermissionNames.All
                .Append(PermissionNames.Wildcard)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }

    private string? ReadHeader(string name)
    {
        return httpContextAccessor.HttpContext?.Request.Headers[name].FirstOrDefault();
    }
}
