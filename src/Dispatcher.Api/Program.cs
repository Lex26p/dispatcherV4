using Dispatcher.Api.Endpoints;
using Dispatcher.Api.Realtime;
using Dispatcher.Application;
using Dispatcher.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddScoped<ITagValueBroadcaster, SignalRTagValueBroadcaster>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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
