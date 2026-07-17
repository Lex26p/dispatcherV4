using Dispatcher.Api.Contracts.Devices;
using Dispatcher.Application.Devices;

namespace Dispatcher.Api.Endpoints;

public static class DeviceEndpoints
{
    public static IEndpointRouteBuilder MapDeviceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/devices")
            .WithTags("Devices");

        group.MapGet("/", async (
            IDeviceService deviceService,
            CancellationToken cancellationToken) =>
        {
            var devices = await deviceService.GetAllAsync(cancellationToken);
            return Results.Ok(devices);
        })
        .WithName("GetDevices");

        group.MapGet("/{id:guid}", async (
            Guid id,
            IDeviceService deviceService,
            CancellationToken cancellationToken) =>
        {
            var device = await deviceService.GetByIdAsync(id, cancellationToken);
            return device is null ? Results.NotFound() : Results.Ok(device);
        })
        .WithName("GetDeviceById");

        group.MapPost("/modbus-tcp", async (
            CreateModbusDeviceRequest request,
            IDeviceService deviceService,
            CancellationToken cancellationToken) =>
        {
            var device = await deviceService.CreateModbusTcpAsync(request.ToCommand(), cancellationToken);
            return Results.Created($"/api/devices/{device.Id}", device);
        })
        .WithName("CreateModbusDevice");

        group.MapPost("/snmp", async (
            CreateSnmpDeviceRequest request,
            IDeviceService deviceService,
            CancellationToken cancellationToken) =>
        {
            var device = await deviceService.CreateSnmpAsync(request.ToCommand(), cancellationToken);
            return Results.Created($"/api/devices/{device.Id}", device);
        })
        .WithName("CreateSnmpDevice");

        group.MapPost("/{id:guid}/enable", async (
            Guid id,
            IDeviceService deviceService,
            CancellationToken cancellationToken) =>
        {
            await deviceService.EnableAsync(id, cancellationToken);
            return Results.NoContent();
        })
        .WithName("EnableDevice");

        group.MapPost("/{id:guid}/disable", async (
            Guid id,
            IDeviceService deviceService,
            CancellationToken cancellationToken) =>
        {
            await deviceService.DisableAsync(id, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DisableDevice");

        return app;
    }
}
