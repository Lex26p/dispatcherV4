using Dispatcher.Api.Security;
using Dispatcher.Application.Abstractions;
using Dispatcher.Application.IdentityAccess;
using Dispatcher.Application.Telemetry.Values;
using Dispatcher.Contracts.Common;
using Dispatcher.Contracts.Telemetry;

namespace Dispatcher.Api.Endpoints.Telemetry;

public static class TelemetryValueEndpoints
{
    public static IEndpointRouteBuilder MapTelemetryValueEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/values").WithTags("Telemetry Values");

        group.MapGet("/current", async (
            Guid? dataPointId,
            Guid? equipmentId,
            Guid? locationId,
            ITelemetryValueService values,
            CancellationToken cancellationToken) =>
        {
            var items = await values.ListCurrentValuesAsync(dataPointId, equipmentId, locationId, cancellationToken);
            return Results.Ok(items);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryValuesView));

        group.MapGet("/current/{dataPointId:guid}", async Task<IResult> (
            Guid dataPointId,
            ITelemetryValueService values,
            CancellationToken cancellationToken) =>
        {
            var item = await values.GetCurrentValueAsync(dataPointId, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryValuesView));

        group.MapPost("/current", async Task<IResult> (
            UpsertCurrentValueRequest request,
            ITelemetryValueService values,
            ICorrelationContext correlationContext,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await values.UpsertCurrentValueAsync(request, cancellationToken);
                return result.Applied
                    ? Results.Created($"/api/values/current/{result.CurrentValue.DataPointId}", result)
                    : Results.Ok(result);
            }
            catch (ArgumentException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
            catch (FormatException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
            catch (OverflowException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.Conflict, "Conflict", StatusCodes.Status409Conflict, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryValuesManage));

        group.MapGet("/history", async Task<IResult> (
            Guid dataPointId,
            DateTimeOffset? fromUtc,
            DateTimeOffset? toUtc,
            int? limit,
            ITelemetryValueService values,
            ICorrelationContext correlationContext,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await values.ListHistoryAsync(dataPointId, fromUtc, toUtc, limit, cancellationToken);
                return Results.Ok(items);
            }
            catch (ArgumentException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.ValidationFailed, "Validation failed", StatusCodes.Status400BadRequest, exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return Problem(correlationContext, ApiErrorCodes.Conflict, "Conflict", StatusCodes.Status409Conflict, exception.Message);
            }
        }).AddEndpointFilter(new RequirePermissionEndpointFilter(PermissionNames.TelemetryValuesView));

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
