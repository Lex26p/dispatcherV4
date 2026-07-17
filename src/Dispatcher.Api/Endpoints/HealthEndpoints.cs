namespace Dispatcher.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", () =>
        {
            return Results.Ok(new
            {
                Status = "Ok",
                Service = "Dispatcher.Api",
                Product = "Диспетчер",
                TimestampUtc = DateTimeOffset.UtcNow
            });
        })
        .WithName("GetHealth")
        .WithTags("Health");

        return app;
    }
}
