using Dispatcher.Api.Security;
using Dispatcher.Application.Abstractions;
using Dispatcher.Application.IdentityAccess;
using Dispatcher.Contracts.Common;
using Dispatcher.Contracts.Identity;
using Dispatcher.Domain.Common;
using Dispatcher.Domain.IdentityAccess;
using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.Api.Endpoints;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api").WithTags("Identity");

        group.MapGet("/me", (ICurrentUser currentUser) => Results.Ok(new MeResponse(
            currentUser.IsAuthenticated,
            currentUser.UserId,
            currentUser.DisplayName,
            currentUser.Permissions.OrderBy(permission => permission, StringComparer.OrdinalIgnoreCase).ToArray())));

        group.MapGet("/users", async (DispatcherDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var users = await dbContext.UserAccounts
                .AsNoTracking()
                .OrderBy(user => user.DisplayName)
                .Select(user => ToDto(user))
                .ToArrayAsync(cancellationToken);

            return Results.Ok(users);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.IdentityUsersView));

        group.MapGet("/users/{id:guid}", async Task<IResult> (Guid id, DispatcherDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var entityId = EntityId.From(id);
            var user = await dbContext.UserAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == entityId, cancellationToken);

            return user is null ? Results.NotFound() : Results.Ok(ToDto(user));
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.IdentityUsersView));

        group.MapGet("/roles", async (DispatcherDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var roles = await dbContext.Roles
                .AsNoTracking()
                .OrderBy(role => role.Code)
                .Select(role => ToDto(role))
                .ToArrayAsync(cancellationToken);

            return Results.Ok(roles);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.IdentityRolesView));

        group.MapGet("/permission-scopes", async (DispatcherDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var scopes = await dbContext.PermissionScopes
                .AsNoTracking()
                .OrderBy(scope => scope.Code)
                .Select(scope => ToDto(scope))
                .ToArrayAsync(cancellationToken);

            return Results.Ok(scopes);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.IdentityScopesView));

        group.MapGet("/role-assignments", async (DispatcherDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var assignments = await QueryAssignments(dbContext)
                .OrderByDescending(assignment => assignment.CreatedAtUtc)
                .ToArrayAsync(cancellationToken);

            return Results.Ok(assignments);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.IdentityAssignmentsView));

        group.MapPost("/role-assignments", async Task<IResult> (
            GrantRoleRequest request,
            DispatcherDbContext dbContext,
            IClock clock,
            CancellationToken cancellationToken) =>
        {
            var userId = EntityId.From(request.UserId);
            var roleId = EntityId.From(request.RoleId);
            EntityId? scopeId = request.ScopeId.HasValue
                ? EntityId.From(request.ScopeId.Value)
                : null;

            var userExists = await dbContext.UserAccounts
                .AnyAsync(user => user.Id == userId && user.IsActive, cancellationToken);

            var roleExists = await dbContext.Roles
                .AnyAsync(role => role.Id == roleId && !role.IsArchived, cancellationToken);

            var scopeExists = !scopeId.HasValue || await dbContext.PermissionScopes
                .AnyAsync(scope => scope.Id == scopeId.Value && !scope.IsArchived, cancellationToken);

            if (!userExists || !roleExists || !scopeExists)
            {
                return Results.NotFound();
            }

            var alreadyActive = await dbContext.RoleAssignments.AnyAsync(
                assignment =>
                    assignment.UserId == userId &&
                    assignment.RoleId == roleId &&
                    assignment.ScopeId == scopeId &&
                    assignment.RevokedAtUtc == null,
                cancellationToken);

            if (alreadyActive)
            {
                return Conflict("role_assignment_already_exists", "This role assignment is already active.");
            }

            var assignment = RoleAssignment.Grant(
                EntityId.New(),
                userId,
                roleId,
                scopeId,
                "manual",
                request.Reason,
                clock.UtcNow);

            dbContext.RoleAssignments.Add(assignment);
            await dbContext.SaveChangesAsync(cancellationToken);

            var dto = await QueryAssignments(dbContext)
                .SingleAsync(item => item.Id == assignment.Id.Value, cancellationToken);

            return Results.Created($"/api/role-assignments/{dto.Id}", dto);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.IdentityAssignmentsManage));

        group.MapPost("/role-assignments/{id:guid}/revoke", async Task<IResult> (
            Guid id,
            RevokeRoleRequest request,
            DispatcherDbContext dbContext,
            IClock clock,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return Results.BadRequest(new
                {
                    code = ApiErrorCodes.ValidationFailed,
                    detail = "Revocation reason is required."
                });
            }

            var entityId = EntityId.From(id);
            var assignment = await dbContext.RoleAssignments
                .SingleOrDefaultAsync(item => item.Id == entityId, cancellationToken);

            if (assignment is null || !assignment.IsActive)
            {
                return Results.NotFound();
            }

            var role = await dbContext.Roles
                .SingleAsync(item => item.Id == assignment.RoleId, cancellationToken);

            if (string.Equals(role.Code, "administrator", StringComparison.OrdinalIgnoreCase))
            {
                var activeAdminCount = await dbContext.RoleAssignments
                    .CountAsync(activeAssignment =>
                        activeAssignment.RevokedAtUtc == null &&
                        activeAssignment.RoleId == role.Id,
                        cancellationToken);

                if (activeAdminCount <= 1)
                {
                    return Conflict("last_admin_guard", "Cannot revoke the last active administrator assignment.");
                }
            }

            assignment.Revoke(clock.UtcNow, request.Reason);
            await dbContext.SaveChangesAsync(cancellationToken);

            var dto = await QueryAssignments(dbContext)
                .SingleAsync(item => item.Id == assignment.Id.Value, cancellationToken);

            return Results.Ok(dto);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.IdentityAssignmentsManage));

        return endpoints;
    }

    private static IQueryable<RoleAssignmentDto> QueryAssignments(DispatcherDbContext dbContext)
    {
        return from assignment in dbContext.RoleAssignments.AsNoTracking()
               join user in dbContext.UserAccounts.AsNoTracking() on assignment.UserId equals user.Id
               join role in dbContext.Roles.AsNoTracking() on assignment.RoleId equals role.Id
               join scopeItem in dbContext.PermissionScopes.AsNoTracking() on assignment.ScopeId equals scopeItem.Id into scopes
               from scope in scopes.DefaultIfEmpty()
               select new RoleAssignmentDto(
                   assignment.Id.Value,
                   user.Id.Value,
                   user.DisplayName,
                   role.Id.Value,
                   role.Code,
                   role.Name,
                   scope == null ? null : scope.Id.Value,
                   scope == null ? null : scope.Code,
                   assignment.Source,
                   assignment.Reason,
                   assignment.CreatedAtUtc,
                   assignment.RevokedAtUtc,
                   assignment.RevokedReason,
                   assignment.RevokedAtUtc == null);
    }

    private static UserAccountDto ToDto(UserAccount user) => new(
        user.Id.Value,
        user.ExternalId,
        user.DisplayName,
        user.Email,
        user.IsActive,
        user.CreatedAtUtc);

    private static RoleDto ToDto(Role role) => new(
        role.Id.Value,
        role.Code,
        role.Name,
        role.Description,
        role.PermissionSet.OrderBy(permission => permission, StringComparer.OrdinalIgnoreCase).ToArray(),
        role.IsSystem,
        role.IsArchived);

    private static PermissionScopeDto ToDto(PermissionScope scope) => new(
        scope.Id.Value,
        scope.Code,
        scope.Name,
        scope.Description,
        scope.IsArchived,
        scope.CreatedAtUtc);

    private static IResult Conflict(string code, string detail)
    {
        return Results.Json(
            new ApiProblemDetails(
                Type: $"https://dispatcher.local/problems/{code}",
                Title: "Conflict",
                Status: StatusCodes.Status409Conflict,
                Code: code,
                Detail: detail,
                CorrelationId: string.Empty),
            statusCode: StatusCodes.Status409Conflict,
            contentType: "application/problem+json");
    }
}
