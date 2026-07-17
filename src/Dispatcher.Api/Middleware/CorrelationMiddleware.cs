using Dispatcher.Application.Abstractions;
using Dispatcher.Contracts.Common;

namespace Dispatcher.Api.Middleware;

public sealed class CorrelationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICorrelationContext correlationContext)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        correlationContext.CorrelationId = correlationId;
        context.TraceIdentifier = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationConstants.HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        await next(context);
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        var incoming = context.Request.Headers[CorrelationConstants.HeaderName]
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(incoming))
        {
            return incoming.Trim();
        }

        return $"disp-{Guid.NewGuid():N}";
    }
}
