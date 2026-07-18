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

    public async Task<IReadOnlyList<EquipmentDto>> GetEquipmentAsync(Guid? locationId, bool includeArchived, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (locationId.HasValue)
        {
            query.Add($"locationId={Uri.EscapeDataString(locationId.Value.ToString())}");
        }

        if (includeArchived)
        {
            query.Add("includeArchived=true");
        }

        var url = query.Count == 0 ? "/api/equipment" : $"/api/equipment?{string.Join('&', query)}";
        return await httpClient.GetFromJsonAsync<EquipmentDto[]>(url, cancellationToken) ?? [];
    }

    public async Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/equipment", request, cancellationToken);
        return await ReadRequiredAsync<EquipmentDto>(response, cancellationToken);
    }

    public async Task<EquipmentDto> UpdateEquipmentAsync(Guid id, UpdateEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync($"/api/equipment/{id}", request, cancellationToken);
        return await ReadRequiredAsync<EquipmentDto>(response, cancellationToken);
    }

    public async Task<EquipmentDto> MoveEquipmentAsync(Guid id, MoveEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync($"/api/equipment/{id}/move", request, cancellationToken);
        return await ReadRequiredAsync<EquipmentDto>(response, cancellationToken);
    }

    public async Task<EquipmentDto> ArchiveEquipmentAsync(Guid id, ArchiveEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync($"/api/equipment/{id}/archive", request, cancellationToken);
        return await ReadRequiredAsync<EquipmentDto>(response, cancellationToken);
    }

    public async Task<EquipmentDto> RestoreEquipmentAsync(Guid id, RestoreEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync($"/api/equipment/{id}/restore", request, cancellationToken);
        return await ReadRequiredAsync<EquipmentDto>(response, cancellationToken);
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
