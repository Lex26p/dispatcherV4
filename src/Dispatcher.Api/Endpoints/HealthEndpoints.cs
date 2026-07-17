using Dispatcher.Application.Abstractions;
using Dispatcher.Contracts.Health;

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

        group.MapGet("/ready", (IClock clock, ICorrelationContext correlation) => Results.Ok(new HealthResponse(
            Status: "Ready",
            Service: "Dispatcher.Api",
            TimestampUtc: clock.UtcNow)));

        return endpoints;
    }
}
