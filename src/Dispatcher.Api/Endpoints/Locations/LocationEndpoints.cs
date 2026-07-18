using Dispatcher.Api.Security;
using Dispatcher.Application.Assets.Locations;
using Dispatcher.Application.Abstractions;
using Dispatcher.Application.IdentityAccess;
using Dispatcher.Contracts.Assets;
using Dispatcher.Contracts.Common;

namespace Dispatcher.Api.Endpoints.Locations;

public static class LocationEndpoints
{
    public static IEndpointRouteBuilder MapLocationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/locations").WithTags("Locations");

        group.MapGet("", async (bool? includeArchived, ILocationService locations, CancellationToken cancellationToken) =>
        {
            var items = await locations.ListAsync(includeArchived == true, cancellationToken);
            return Results.Ok(items);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.LocationsView));

        group.MapGet("/{id:guid}", async Task<IResult> (Guid id, ILocationService locations, CancellationToken cancellationToken) =>
        {
            var location = await locations.GetAsync(id, cancellationToken);
            return location is null ? Results.NotFound() : Results.Ok(location);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.LocationsView));

        group.MapPost("", async Task<IResult> (
            CreateLocationRequest request,
            ILocationService locations,
            ICorrelationContext correlationContext,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await locations.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/locations/{created.Id}", created);
            }
            catch (ArgumentException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.Conflict, "Conflict", StatusCodes.Status409Conflict, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.LocationsManage));

        group.MapPut("/{id:guid}", async Task<IResult> (
            Guid id,
            UpdateLocationRequest request,
            ILocationService locations,
            ICorrelationContext correlationContext,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await locations.UpdateAsync(id, request, cancellationToken);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            }
            catch (ArgumentException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.LocationsManage));

        group.MapPost("/{id:guid}/move", async Task<IResult> (
            Guid id,
            MoveLocationRequest request,
            ILocationService locations,
            ICorrelationContext correlationContext,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var moved = await locations.MoveAsync(id, request, cancellationToken);
                return moved is null ? Results.NotFound() : Results.Ok(moved);
            }
            catch (ArgumentException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.Conflict, "Conflict", StatusCodes.Status409Conflict, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.LocationsManage));

        group.MapPost("/{id:guid}/archive", async Task<IResult> (
            Guid id,
            ArchiveLocationRequest request,
            ILocationService locations,
            ICorrelationContext correlationContext,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var archived = await locations.ArchiveAsync(id, request, cancellationToken);
                return archived is null ? Results.NotFound() : Results.Ok(archived);
            }
            catch (InvalidOperationException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.Conflict, "Conflict", StatusCodes.Status409Conflict, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.LocationsManage));

        return endpoints;
    }

    private static IResult Problem(
        ICorrelationContext correlationContext,
        string code,
        string title,
        int status,
        string detail)
    {
        return Results.Json(
            new ApiProblemDetails(
                Type: $"https://dispatcher.local/problems/{code}",
                Title: title,
                Status: status,
                Code: code,
                Detail: detail,
                CorrelationId: correlationContext.CorrelationId),
            statusCode: status,
            contentType: "application/problem+json");
    }
}
