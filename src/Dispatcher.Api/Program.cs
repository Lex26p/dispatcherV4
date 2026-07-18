using Dispatcher.Api.Configuration;
using Dispatcher.Api.Endpoints;
using Dispatcher.Api.Endpoints.Equipment;
using Dispatcher.Api.Endpoints.Locations;
using Dispatcher.Api.Endpoints.Telemetry;
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
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentLocalhost", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                    && (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase));
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentLocalhost");
}

app.MapGet("/", () => Results.Redirect("/api/health/live"));
app.MapHealthEndpoints();
app.MapIdentityEndpoints();
app.MapLocationEndpoints();
app.MapEquipmentEndpoints();
app.MapTelemetryConfigurationEndpoints();
app.MapTelemetryValueEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapDiagnosticsEndpoints();
}

app.Run();

public partial class Program;
