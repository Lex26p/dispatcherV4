using System.Net;
using System.Net.Http.Json;
using Dispatcher.Contracts.Assets;
using Dispatcher.Contracts.Telemetry;

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

    public async Task<IReadOnlyList<TelemetrySourceDto>> GetTelemetrySourcesAsync(bool includeArchived, CancellationToken cancellationToken = default)
    {
        var url = includeArchived ? "/api/telemetry-sources?includeArchived=true" : "/api/telemetry-sources";
        return await httpClient.GetFromJsonAsync<TelemetrySourceDto[]>(url, cancellationToken) ?? [];
    }

    public async Task<TelemetrySourceDto> CreateTelemetrySourceAsync(CreateTelemetrySourceRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/telemetry-sources", request, cancellationToken);
        return await ReadRequiredAsync<TelemetrySourceDto>(response, cancellationToken);
    }

    public async Task<TelemetrySourceDto> EnableTelemetrySourceAsync(Guid id, TelemetrySourceActionRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync($"/api/telemetry-sources/{id}/enable", request, cancellationToken);
        return await ReadRequiredAsync<TelemetrySourceDto>(response, cancellationToken);
    }

    public async Task<TelemetrySourceDto> DisableTelemetrySourceAsync(Guid id, TelemetrySourceActionRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync($"/api/telemetry-sources/{id}/disable", request, cancellationToken);
        return await ReadRequiredAsync<TelemetrySourceDto>(response, cancellationToken);
    }

    public async Task<TelemetrySourceDto> ArchiveTelemetrySourceAsync(Guid id, TelemetrySourceActionRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync($"/api/telemetry-sources/{id}/archive", request, cancellationToken);
        return await ReadRequiredAsync<TelemetrySourceDto>(response, cancellationToken);
    }

    public async Task<TelemetrySourceDto> RestoreTelemetrySourceAsync(Guid id, TelemetrySourceActionRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync($"/api/telemetry-sources/{id}/restore", request, cancellationToken);
        return await ReadRequiredAsync<TelemetrySourceDto>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<DataPointDto>> GetDataPointsAsync(Guid? equipmentId, bool includeArchived, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (equipmentId.HasValue)
        {
            query.Add($"equipmentId={Uri.EscapeDataString(equipmentId.Value.ToString())}");
        }

        if (includeArchived)
        {
            query.Add("includeArchived=true");
        }

        var url = query.Count == 0 ? "/api/data-points" : $"/api/data-points?{string.Join('&', query)}";
        return await httpClient.GetFromJsonAsync<DataPointDto[]>(url, cancellationToken) ?? [];
    }

    public async Task<DataPointDto> CreateDataPointAsync(CreateDataPointRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/data-points", request, cancellationToken);
        return await ReadRequiredAsync<DataPointDto>(response, cancellationToken);
    }

    public async Task<DataPointDto> ArchiveDataPointAsync(Guid id, DataPointActionRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync($"/api/data-points/{id}/archive", request, cancellationToken);
        return await ReadRequiredAsync<DataPointDto>(response, cancellationToken);
    }

    public async Task<DataPointDto> RestoreDataPointAsync(Guid id, DataPointActionRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync($"/api/data-points/{id}/restore", request, cancellationToken);
        return await ReadRequiredAsync<DataPointDto>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<ProtocolMappingDto>> GetProtocolMappingsAsync(Guid? dataPointId, Guid? telemetrySourceId, bool includeArchived, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (dataPointId.HasValue)
        {
            query.Add($"dataPointId={Uri.EscapeDataString(dataPointId.Value.ToString())}");
        }

        if (telemetrySourceId.HasValue)
        {
            query.Add($"telemetrySourceId={Uri.EscapeDataString(telemetrySourceId.Value.ToString())}");
        }

        if (includeArchived)
        {
            query.Add("includeArchived=true");
        }

        var url = query.Count == 0 ? "/api/protocol-mappings" : $"/api/protocol-mappings?{string.Join('&', query)}";
        return await httpClient.GetFromJsonAsync<ProtocolMappingDto[]>(url, cancellationToken) ?? [];
    }

    public async Task<ProtocolMappingDto> CreateProtocolMappingAsync(CreateProtocolMappingRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/protocol-mappings", request, cancellationToken);
        return await ReadRequiredAsync<ProtocolMappingDto>(response, cancellationToken);
    }

    public async Task<ProtocolMappingDto> ArchiveProtocolMappingAsync(Guid id, ProtocolMappingActionRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync($"/api/protocol-mappings/{id}/archive", request, cancellationToken);
        return await ReadRequiredAsync<ProtocolMappingDto>(response, cancellationToken);
    }

    public async Task<ProtocolMappingDto> RestoreProtocolMappingAsync(Guid id, ProtocolMappingActionRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync($"/api/protocol-mappings/{id}/restore", request, cancellationToken);
        return await ReadRequiredAsync<ProtocolMappingDto>(response, cancellationToken);
    }


    public async Task<IReadOnlyList<CurrentValueDto>> GetCurrentValuesAsync(Guid? dataPointId, Guid? equipmentId, Guid? locationId, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (dataPointId.HasValue)
        {
            query.Add($"dataPointId={Uri.EscapeDataString(dataPointId.Value.ToString())}");
        }

        if (equipmentId.HasValue)
        {
            query.Add($"equipmentId={Uri.EscapeDataString(equipmentId.Value.ToString())}");
        }

        if (locationId.HasValue)
        {
            query.Add($"locationId={Uri.EscapeDataString(locationId.Value.ToString())}");
        }

        var url = query.Count == 0 ? "/api/values/current" : $"/api/values/current?{string.Join('&', query)}";
        return await httpClient.GetFromJsonAsync<CurrentValueDto[]>(url, cancellationToken) ?? [];
    }

    public async Task<CurrentValueDto?> GetCurrentValueAsync(Guid dataPointId, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"/api/values/current/{dataPointId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        return await ReadRequiredAsync<CurrentValueDto>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<HistoricalValueDto>> GetHistoricalValuesAsync(Guid dataPointId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, int? limit, CancellationToken cancellationToken = default)
    {
        var query = new List<string>
        {
            $"dataPointId={Uri.EscapeDataString(dataPointId.ToString())}"
        };

        if (fromUtc.HasValue)
        {
            query.Add($"fromUtc={Uri.EscapeDataString(fromUtc.Value.ToString("O"))}");
        }

        if (toUtc.HasValue)
        {
            query.Add($"toUtc={Uri.EscapeDataString(toUtc.Value.ToString("O"))}");
        }

        if (limit.HasValue)
        {
            query.Add($"limit={limit.Value}");
        }

        var url = $"/api/values/history?{string.Join('&', query)}";
        return await httpClient.GetFromJsonAsync<HistoricalValueDto[]>(url, cancellationToken) ?? [];
    }

    public async Task<UpsertCurrentValueResponse> UpsertCurrentValueAsync(UpsertCurrentValueRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/values/current", request, cancellationToken);
        return await ReadRequiredAsync<UpsertCurrentValueResponse>(response, cancellationToken);
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
