using Dispatcher.Api.Endpoints;
using Dispatcher.Api.Middleware;
using Dispatcher.Application;
using Dispatcher.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDispatcherApplication();
builder.Services.AddDispatcherInfrastructure();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapGet("/", () => Results.Redirect("/api/health/live"));
app.MapHealthEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapDiagnosticsEndpoints();
}

app.Run();

public partial class Program;
