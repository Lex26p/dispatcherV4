using Dispatcher.Application.Abstractions;
using Dispatcher.Contracts.Health;
using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/health").WithTags("Health");

        group.MapGet("/live", (IClock clock, ICorrelationContext correlation) => Results.Ok(new HealthResponse(
            Status: "Live",
            Service: "Dispatcher.Api",
            TimestampUtc: clock.UtcNow)));

        group.MapGet("/ready", async (IClock clock, DispatcherDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var timestamp = clock.UtcNow;

            try
            {
                var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

                if (!canConnect)
                {
                    return Results.Json(
                        new ReadinessResponse(
                            Status: "NotReady",
                            Service: "Dispatcher.Api",
                            TimestampUtc: timestamp,
                            Dependencies:
                            [
                                new HealthDependencyStatus("postgresql", "Unavailable", "Database connection check returned false")
                            ]),
                        statusCode: StatusCodes.Status503ServiceUnavailable);
                }

                return Results.Ok(new ReadinessResponse(
                    Status: "Ready",
                    Service: "Dispatcher.Api",
                    TimestampUtc: timestamp,
                    Dependencies:
                    [
                        new HealthDependencyStatus("postgresql", "Ready")
                    ]));
            }
            catch (Exception ex) when (ex is Npgsql.NpgsqlException or InvalidOperationException or TimeoutException)
            {
                return Results.Json(
                    new ReadinessResponse(
                        Status: "NotReady",
                        Service: "Dispatcher.Api",
                        TimestampUtc: timestamp,
                        Dependencies:
                        [
                            new HealthDependencyStatus("postgresql", "Unavailable", "Database connection failed")
                        ]),
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        });

        return endpoints;
    }
}
