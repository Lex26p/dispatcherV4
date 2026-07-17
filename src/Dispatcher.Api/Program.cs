using Dispatcher.Api.Endpoints;
using Dispatcher.Application;
using Dispatcher.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDispatcherApplication();
builder.Services.AddDispatcherInfrastructure();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("/api/health/live"));
app.MapHealthEndpoints();

app.Run();

public partial class Program;
