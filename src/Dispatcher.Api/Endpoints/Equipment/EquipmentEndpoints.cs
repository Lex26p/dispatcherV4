using Dispatcher.Api.Security;
using Dispatcher.Application.Abstractions;
using Dispatcher.Application.Assets.Equipment;
using Dispatcher.Application.IdentityAccess;
using Dispatcher.Contracts.Assets;
using Dispatcher.Contracts.Common;

namespace Dispatcher.Api.Endpoints.Equipment;

public static class EquipmentEndpoints
{
    public static IEndpointRouteBuilder MapEquipmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/equipment").WithTags("Equipment");

        group.MapGet("", async (Guid? locationId, bool? includeArchived, IEquipmentService equipment, CancellationToken cancellationToken) =>
        {
            var items = await equipment.ListAsync(locationId, includeArchived == true, cancellationToken);
            return Results.Ok(items);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.EquipmentView));

        group.MapGet("/{id:guid}", async Task<IResult> (Guid id, IEquipmentService equipment, CancellationToken cancellationToken) =>
        {
            var item = await equipment.GetAsync(id, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.EquipmentView));

        group.MapPost("", async Task<IResult> (
            CreateEquipmentRequest request,
            IEquipmentService equipment,
            ICorrelationContext correlationContext,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await equipment.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/equipment/{created.Id}", created);
            }
            catch (ArgumentException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.Conflict, "Conflict", StatusCodes.Status409Conflict, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.EquipmentManage));

        group.MapPut("/{id:guid}", async Task<IResult> (
            Guid id,
            UpdateEquipmentRequest request,
            IEquipmentService equipment,
            ICorrelationContext correlationContext,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await equipment.UpdateAsync(id, request, cancellationToken);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            }
            catch (ArgumentException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.EquipmentManage));

        group.MapPost("/{id:guid}/move", async Task<IResult> (
            Guid id,
            MoveEquipmentRequest request,
            IEquipmentService equipment,
            ICorrelationContext correlationContext,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var moved = await equipment.MoveAsync(id, request, cancellationToken);
                return moved is null ? Results.NotFound() : Results.Ok(moved);
            }
            catch (InvalidOperationException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.Conflict, "Conflict", StatusCodes.Status409Conflict, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.EquipmentManage));

        group.MapPost("/{id:guid}/archive", async Task<IResult> (
            Guid id,
            ArchiveEquipmentRequest request,
            IEquipmentService equipment,
            CancellationToken cancellationToken) =>
        {
            var archived = await equipment.ArchiveAsync(id, request, cancellationToken);
            return archived is null ? Results.NotFound() : Results.Ok(archived);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.EquipmentManage));

        group.MapPost("/{id:guid}/restore", async Task<IResult> (
            Guid id,
            RestoreEquipmentRequest request,
            IEquipmentService equipment,
            ICorrelationContext correlationContext,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var restored = await equipment.RestoreAsync(id, request, cancellationToken);
                return restored is null ? Results.NotFound() : Results.Ok(restored);
            }
            catch (InvalidOperationException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.Conflict, "Conflict", StatusCodes.Status409Conflict, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.EquipmentManage));

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
