using Dispatcher.Api.Contracts.Tags;
using Dispatcher.Application.Tags;

namespace Dispatcher.Api.Endpoints;

public static class TagEndpoints
{
    public static IEndpointRouteBuilder MapTagEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tags")
            .WithTags("Tags");

        group.MapGet("/{id:guid}", async (
            Guid id,
            ITagService tagService,
            CancellationToken cancellationToken) =>
        {
            var tag = await tagService.GetByIdAsync(id, cancellationToken);
            return tag is null ? Results.NotFound() : Results.Ok(tag);
        })
        .WithName("GetTagById");

        group.MapPost("/modbus", async (
            CreateModbusTagRequest request,
            ITagService tagService,
            CancellationToken cancellationToken) =>
        {
            var tag = await tagService.CreateModbusTagAsync(request.ToCommand(), cancellationToken);
            return Results.Created($"/api/tags/{tag.Id}", tag);
        })
        .WithName("CreateModbusTag");

        group.MapPost("/snmp", async (
            CreateSnmpTagRequest request,
            ITagService tagService,
            CancellationToken cancellationToken) =>
        {
            var tag = await tagService.CreateSnmpTagAsync(request.ToCommand(), cancellationToken);
            return Results.Created($"/api/tags/{tag.Id}", tag);
        })
        .WithName("CreateSnmpTag");

        group.MapPost("/{id:guid}/enable", async (
            Guid id,
            ITagService tagService,
            CancellationToken cancellationToken) =>
        {
            await tagService.EnableAsync(id, cancellationToken);
            return Results.NoContent();
        })
        .WithName("EnableTag");

        group.MapPost("/{id:guid}/disable", async (
            Guid id,
            ITagService tagService,
            CancellationToken cancellationToken) =>
        {
            await tagService.DisableAsync(id, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DisableTag");

        var deviceTagsGroup = app.MapGroup("/api/devices/{deviceId:guid}/tags")
            .WithTags("Tags");

        deviceTagsGroup.MapGet("/", async (
            Guid deviceId,
            ITagService tagService,
            CancellationToken cancellationToken) =>
        {
            var tags = await tagService.GetByDeviceIdAsync(deviceId, cancellationToken);
            return Results.Ok(tags);
        })
        .WithName("GetTagsByDeviceId");

        return app;
    }
}
