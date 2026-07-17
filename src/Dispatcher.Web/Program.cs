using Dispatcher.Web;
using Dispatcher.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

var configuredApiBaseUrl = builder.Configuration["ApiBaseUrl"];

if (!Uri.TryCreate(configuredApiBaseUrl, UriKind.Absolute, out var dispatcherApiBaseAddress))
{
    dispatcherApiBaseAddress = new Uri("http://localhost:5076/");
}

builder.Services.AddScoped(_ => new DispatcherApiClient(new HttpClient
{
    BaseAddress = dispatcherApiBaseAddress
}));

builder.Services.AddScoped<TagValueRealtimeClient>();

await builder.Build().RunAsync();
