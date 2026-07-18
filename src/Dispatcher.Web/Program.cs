using Dispatcher.Web;
using Dispatcher.Web.Api;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var apiBaseUrl = configuration["DispatcherApi:BaseUrl"] ?? "http://localhost:5076/";

    return new DispatcherApiClient(new HttpClient
    {
        BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute)
    });
});

await builder.Build().RunAsync();
