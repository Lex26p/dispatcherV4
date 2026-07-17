using Dispatcher.Application;
using Dispatcher.Infrastructure;
using Dispatcher.Worker.MockPolling;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<MockPollingOptions>(
    builder.Configuration.GetSection(MockPollingOptions.SectionName));
builder.Services.AddHostedService<MockPollingWorker>();

var host = builder.Build();
host.Run();
