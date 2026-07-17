using Dispatcher.Api.Endpoints;
using Dispatcher.Api.Realtime;
using Dispatcher.Application;
using Dispatcher.Infrastructure;

const string blazorDevelopmentCorsPolicy = "BlazorDevelopment";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddScoped<ITagValueBroadcaster, SignalRTagValueBroadcaster>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(blazorDevelopmentCorsPolicy, policy =>
    {
        policy.WithOrigins(
                "http://localhost:5048",
                "https://localhost:7201")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors(blazorDevelopmentCorsPolicy);
}
else
{
    app.UseHttpsRedirection();
}

app.MapHealthEndpoints();
app.MapDeviceEndpoints();
app.MapTagEndpoints();
app.MapTagValueEndpoints();
app.MapRealtimeEndpoints();
app.MapHub<TagValueHub>(RealtimeConstants.TagValuesHubPath);

app.Run();
