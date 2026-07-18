using Dispatcher.Api.Configuration;
using Dispatcher.Api.Endpoints;
using Dispatcher.Api.Middleware;
using Dispatcher.Api.Security;
using Dispatcher.Application;
using Dispatcher.Application.Abstractions;
using Dispatcher.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var dispatcherDatabaseConnectionString = builder.Configuration.GetDispatcherDatabaseConnectionString();

builder.Services.AddDispatcherApplication();
builder.Services.AddDispatcherInfrastructure(dispatcherDatabaseConnectionString);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpHeaderCurrentUser>();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapGet("/", () => Results.Redirect("/api/health/live"));
app.MapHealthEndpoints();
app.MapIdentityEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapDiagnosticsEndpoints();
}

app.Run();

public partial class Program;
