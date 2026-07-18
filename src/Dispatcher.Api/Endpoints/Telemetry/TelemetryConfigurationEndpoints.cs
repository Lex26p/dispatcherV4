using Dispatcher.Api.Security;
using Dispatcher.Application.Abstractions;
using Dispatcher.Application.IdentityAccess;
using Dispatcher.Application.Telemetry.Configuration;
using Dispatcher.Contracts.Common;
using Dispatcher.Contracts.Telemetry;

namespace Dispatcher.Api.Endpoints.Telemetry;

public static class TelemetryConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapTelemetryConfigurationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var sourceGroup = endpoints.MapGroup("/api/telemetry-sources").WithTags("Telemetry Sources");

        sourceGroup.MapGet("", async (bool? includeArchived, ITelemetryConfigurationService telemetry, CancellationToken cancellationToken) =>
        {
            var items = await telemetry.ListSourcesAsync(includeArchived == true, cancellationToken);
            return Results.Ok(items);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationView));

        sourceGroup.MapGet("/{id:guid}", async Task<IResult> (Guid id, ITelemetryConfigurationService telemetry, CancellationToken cancellationToken) =>
        {
            var item = await telemetry.GetSourceAsync(id, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationView));

        sourceGroup.MapPost("", async Task<IResult> (
            CreateTelemetrySourceRequest request,
            ITelemetryConfigurationService telemetry,
            ICorrelationContext correlationContext,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await telemetry.CreateSourceAsync(request, cancellationToken);
                return Results.Created($"/api/telemetry-sources/{created.Id}", created);
            }
            catch (ArgumentException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.Conflict, "Conflict", StatusCodes.Status409Conflict, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationManage));

        sourceGroup.MapPut("/{id:guid}", async Task<IResult> (
            Guid id,
            UpdateTelemetrySourceRequest request,
            ITelemetryConfigurationService telemetry,
            ICorrelationContext correlationContext,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await telemetry.UpdateSourceAsync(id, request, cancellationToken);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            }
            catch (ArgumentException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationManage));

        sourceGroup.MapPost("/{id:guid}/enable", async Task<IResult> (Guid id, TelemetrySourceActionRequest request, ITelemetryConfigurationService telemetry, ICorrelationContext correlationContext, CancellationToken cancellationToken) =>
        {
            try
            {
                var item = await telemetry.EnableSourceAsync(id, request, cancellationToken);
                return item is null ? Results.NotFound() : Results.Ok(item);
            }
            catch (InvalidOperationException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.Conflict, "Conflict", StatusCodes.Status409Conflict, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationManage));

        sourceGroup.MapPost("/{id:guid}/disable", async Task<IResult> (Guid id, TelemetrySourceActionRequest request, ITelemetryConfigurationService telemetry, CancellationToken cancellationToken) =>
        {
            var item = await telemetry.DisableSourceAsync(id, request, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationManage));

        sourceGroup.MapPost("/{id:guid}/archive", async Task<IResult> (Guid id, TelemetrySourceActionRequest request, ITelemetryConfigurationService telemetry, CancellationToken cancellationToken) =>
        {
            var item = await telemetry.ArchiveSourceAsync(id, request, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationManage));

        sourceGroup.MapPost("/{id:guid}/restore", async Task<IResult> (Guid id, TelemetrySourceActionRequest request, ITelemetryConfigurationService telemetry, CancellationToken cancellationToken) =>
        {
            var item = await telemetry.RestoreSourceAsync(id, request, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationManage));

        var pointGroup = endpoints.MapGroup("/api/data-points").WithTags("Data Points");

        pointGroup.MapGet("", async (Guid? equipmentId, bool? includeArchived, ITelemetryConfigurationService telemetry, CancellationToken cancellationToken) =>
        {
            var items = await telemetry.ListDataPointsAsync(equipmentId, includeArchived == true, cancellationToken);
            return Results.Ok(items);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationView));

        pointGroup.MapGet("/{id:guid}", async Task<IResult> (Guid id, ITelemetryConfigurationService telemetry, CancellationToken cancellationToken) =>
        {
            var item = await telemetry.GetDataPointAsync(id, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationView));

        pointGroup.MapPost("", async Task<IResult> (CreateDataPointRequest request, ITelemetryConfigurationService telemetry, ICorrelationContext correlationContext, CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await telemetry.CreateDataPointAsync(request, cancellationToken);
                return Results.Created($"/api/data-points/{created.Id}", created);
            }
            catch (ArgumentException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.Conflict, "Conflict", StatusCodes.Status409Conflict, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationManage));

        pointGroup.MapPut("/{id:guid}", async Task<IResult> (Guid id, UpdateDataPointRequest request, ITelemetryConfigurationService telemetry, ICorrelationContext correlationContext, CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await telemetry.UpdateDataPointAsync(id, request, cancellationToken);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            }
            catch (ArgumentException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationManage));

        pointGroup.MapPost("/{id:guid}/archive", async Task<IResult> (Guid id, DataPointActionRequest request, ITelemetryConfigurationService telemetry, CancellationToken cancellationToken) =>
        {
            var item = await telemetry.ArchiveDataPointAsync(id, request, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationManage));

        pointGroup.MapPost("/{id:guid}/restore", async Task<IResult> (Guid id, DataPointActionRequest request, ITelemetryConfigurationService telemetry, ICorrelationContext correlationContext, CancellationToken cancellationToken) =>
        {
            try
            {
                var item = await telemetry.RestoreDataPointAsync(id, request, cancellationToken);
                return item is null ? Results.NotFound() : Results.Ok(item);
            }
            catch (InvalidOperationException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.Conflict, "Conflict", StatusCodes.Status409Conflict, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationManage));

        var mappingGroup = endpoints.MapGroup("/api/protocol-mappings").WithTags("Protocol Mappings");

        mappingGroup.MapGet("", async (Guid? dataPointId, Guid? telemetrySourceId, bool? includeArchived, ITelemetryConfigurationService telemetry, CancellationToken cancellationToken) =>
        {
            var items = await telemetry.ListMappingsAsync(dataPointId, telemetrySourceId, includeArchived == true, cancellationToken);
            return Results.Ok(items);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationView));

        mappingGroup.MapGet("/{id:guid}", async Task<IResult> (Guid id, ITelemetryConfigurationService telemetry, CancellationToken cancellationToken) =>
        {
            var item = await telemetry.GetMappingAsync(id, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationView));

        mappingGroup.MapPost("", async Task<IResult> (CreateProtocolMappingRequest request, ITelemetryConfigurationService telemetry, ICorrelationContext correlationContext, CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await telemetry.CreateMappingAsync(request, cancellationToken);
                return Results.Created($"/api/protocol-mappings/{created.Id}", created);
            }
            catch (ArgumentException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.Conflict, "Conflict", StatusCodes.Status409Conflict, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationManage));

        mappingGroup.MapPut("/{id:guid}", async Task<IResult> (Guid id, UpdateProtocolMappingRequest request, ITelemetryConfigurationService telemetry, ICorrelationContext correlationContext, CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await telemetry.UpdateMappingAsync(id, request, cancellationToken);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            }
            catch (ArgumentException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationManage));

        mappingGroup.MapPost("/{id:guid}/archive", async Task<IResult> (Guid id, ProtocolMappingActionRequest request, ITelemetryConfigurationService telemetry, CancellationToken cancellationToken) =>
        {
            var item = await telemetry.ArchiveMappingAsync(id, request, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationManage));

        mappingGroup.MapPost("/{id:guid}/restore", async Task<IResult> (Guid id, ProtocolMappingActionRequest request, ITelemetryConfigurationService telemetry, CancellationToken cancellationToken) =>
        {
            var item = await telemetry.RestoreMappingAsync(id, request, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryConfigurationManage));

        return endpoints;
    }

    private static IResult Problem(ICorrelationContext correlationContext, string code, string title, int status, string detail)
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
