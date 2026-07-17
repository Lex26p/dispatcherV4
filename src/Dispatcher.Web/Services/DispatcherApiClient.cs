using System.Net.Http.Json;
using Dispatcher.Web.Models;

namespace Dispatcher.Web.Services;

public sealed class DispatcherApiClient
{
    private readonly HttpClient httpClient;

    public DispatcherApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Uri ApiBaseAddress => httpClient.BaseAddress ?? new Uri("http://localhost:5076/");

    public async Task<ApiHealthResponse?> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<ApiHealthResponse>("api/health", cancellationToken);
    }

    public async Task<RealtimeInfoResponse?> GetRealtimeInfoAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<RealtimeInfoResponse>("api/realtime", cancellationToken);
    }
}
