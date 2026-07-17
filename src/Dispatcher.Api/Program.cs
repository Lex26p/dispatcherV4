using Dispatcher.Api.Endpoints;
using Dispatcher.Application;
using Dispatcher.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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

app.Run();
