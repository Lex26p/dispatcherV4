using System.Net;
using System.Net.Http.Json;
using Dispatcher.Contracts.Assets;

namespace Dispatcher.Web.Api;

public sealed class DispatcherApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<LocationDto>> GetLocationsAsync(bool includeArchived, CancellationToken cancellationToken = default)
    {
        var url = includeArchived ? "/api/locations?includeArchived=true" : "/api/locations";
        return await httpClient.GetFromJsonAsync<LocationDto[]>(url, cancellationToken) ?? [];
    }

    public async Task<LocationDto> CreateLocationAsync(CreateLocationRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/locations", request, cancellationToken);
        return await ReadRequiredAsync<LocationDto>(response, cancellationToken);
    }

    public async Task<LocationDto> UpdateLocationAsync(Guid id, UpdateLocationRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync($"/api/locations/{id}", request, cancellationToken);
        return await ReadRequiredAsync<LocationDto>(response, cancellationToken);
    }

    public async Task<LocationDto> ArchiveLocationAsync(Guid id, ArchiveLocationRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync($"/api/locations/{id}/archive", request, cancellationToken);
        return await ReadRequiredAsync<LocationDto>(response, cancellationToken);
    }

    private static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var value = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            if (value is not null)
            {
                return value;
            }

            throw new DispatcherApiException(response.StatusCode, "API returned an empty response.");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new DispatcherApiException(response.StatusCode, body);
    }
}

public sealed class DispatcherApiException(HttpStatusCode statusCode, string responseBody)
    : Exception($"Dispatcher API request failed with status {(int)statusCode}.")
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    public string ResponseBody { get; } = responseBody;
}
