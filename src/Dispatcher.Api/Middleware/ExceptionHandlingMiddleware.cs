using Dispatcher.Application.Abstractions;
using Dispatcher.Contracts.Common;

namespace Dispatcher.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, ICorrelationContext correlationContext)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var correlationId = string.IsNullOrWhiteSpace(correlationContext.CorrelationId)
                ? context.TraceIdentifier
                : correlationContext.CorrelationId;

            logger.LogError(
                exception,
                "Unhandled API exception. CorrelationId: {CorrelationId}",
                correlationId);

            var problem = new ApiProblemDetails(
                Type: "https://dispatcher.local/problems/internal-server-error",
                Title: "Internal server error",
                Status: StatusCodes.Status500InternalServerError,
                Code: ApiErrorCodes.InternalServerError,
                Detail: "The request failed. Use the correlation ID to find server-side diagnostics.",
                CorrelationId: correlationId);

            context.Response.Clear();
            context.Response.StatusCode = problem.Status;
            context.Response.ContentType = "application/problem+json";
            context.Response.Headers[CorrelationConstants.HeaderName] = correlationId;

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
