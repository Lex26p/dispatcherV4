using Dispatcher.Api.Contracts.Tags;
using Dispatcher.Application.Tags;

namespace Dispatcher.Api.Endpoints;

public static class TagValueEndpoints
{
    public static IEndpointRouteBuilder MapTagValueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tag-values")
            .WithTags("Tag Values");

        group.MapGet("/current", async (
            ITagValueService tagValueService,
            CancellationToken cancellationToken) =>
        {
            var values = await tagValueService.GetCurrentValuesAsync(cancellationToken);
            return Results.Ok(values);
        })
        .WithName("GetCurrentTagValues");

        group.MapGet("/current/{tagId:guid}", async (
            Guid tagId,
            ITagValueService tagValueService,
            CancellationToken cancellationToken) =>
        {
            var value = await tagValueService.GetCurrentValueAsync(tagId, cancellationToken);
            return value is null ? Results.NotFound() : Results.Ok(value);
        })
        .WithName("GetCurrentTagValue");

        group.MapPost("/current", async (
            UpsertTagValueRequest request,
            ITagValueService tagValueService,
            CancellationToken cancellationToken) =>
        {
            await tagValueService.UpsertCurrentValueAsync(request.ToDto(), cancellationToken);
            return Results.NoContent();
        })
        .WithName("UpsertCurrentTagValue");

        group.MapPost("/current/{tagId:guid}", async (
            Guid tagId,
            UpsertTagValueRequest request,
            ITagValueService tagValueService,
            CancellationToken cancellationToken) =>
        {
            await tagValueService.UpsertCurrentValueAsync(request.ToDto(tagId), cancellationToken);
            return Results.NoContent();
        })
        .WithName("UpsertCurrentTagValueByTagId");

        app.MapGet("/api/tags/{tagId:guid}/current-value", async (
            Guid tagId,
            ITagValueService tagValueService,
            CancellationToken cancellationToken) =>
        {
            var value = await tagValueService.GetCurrentValueAsync(tagId, cancellationToken);
            return value is null ? Results.NotFound() : Results.Ok(value);
        })
        .WithTags("Tag Values")
        .WithName("GetTagCurrentValue");

        return app;
    }
}
