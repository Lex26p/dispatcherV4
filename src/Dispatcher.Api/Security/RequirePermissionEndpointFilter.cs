using Dispatcher.Application.Abstractions;
using Dispatcher.Application.IdentityAccess;
using Dispatcher.Contracts.Common;

namespace Dispatcher.Api.Security;

public sealed class RequirePermissionEndpointFilter(string permission) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
        if (!currentUser.IsAuthenticated)
        {
            return Problem(context.HttpContext, ApiErrorCodes.Unauthorized, "Unauthorized", StatusCodes.Status401Unauthorized);
        }

        if (!currentUser.Permissions.Contains(PermissionNames.Wildcard) && !currentUser.Permissions.Contains(permission))
        {
            return Problem(context.HttpContext, ApiErrorCodes.Forbidden, "Forbidden", StatusCodes.Status403Forbidden);
        }

        return await next(context);
    }

    private static IResult Problem(HttpContext httpContext, string code, string title, int status)
    {
        var correlation = httpContext.RequestServices.GetRequiredService<ICorrelationContext>();
        var correlationId = string.IsNullOrWhiteSpace(correlation.CorrelationId)
            ? httpContext.TraceIdentifier
            : correlation.CorrelationId;

        return Results.Json(
            new ApiProblemDetails(
                Type: $"https://dispatcher.local/problems/{code}",
                Title: title,
                Status: status,
                Code: code,
                Detail: "The current user is not allowed to perform this operation.",
                CorrelationId: correlationId),
            statusCode: status,
            contentType: "application/problem+json");
    }
}
