using System.Net.Http.Json;
using System.Text.Json;
using Dispatcher.Web.Models;

namespace Dispatcher.Web.Services;

public sealed class DispatcherApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient httpClient;

    public DispatcherApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Uri ApiBaseAddress => httpClient.BaseAddress ?? new Uri("http://localhost:5076/");

    public async Task<ApiHealthResponse?> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<ApiHealthResponse>("api/health", JsonOptions, cancellationToken);
    }

    public async Task<RealtimeInfoResponse?> GetRealtimeInfoAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<RealtimeInfoResponse>("api/realtime", JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<DeviceDto>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        var devices = await httpClient.GetFromJsonAsync<DeviceDto[]>("api/devices", JsonOptions, cancellationToken);
        return devices ?? Array.Empty<DeviceDto>();
    }

    public async Task<DeviceDto?> GetDeviceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<DeviceDto>($"api/devices/{id}", JsonOptions, cancellationToken);
    }

    public async Task<DeviceDto> CreateModbusDeviceAsync(
        CreateModbusDeviceRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/devices/modbus-tcp", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await ReadRequiredAsync<DeviceDto>(response, cancellationToken);
    }

    public async Task<DeviceDto> CreateSnmpDeviceAsync(
        CreateSnmpDeviceRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/devices/snmp", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await ReadRequiredAsync<DeviceDto>(response, cancellationToken);
    }

    public async Task EnableDeviceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync($"api/devices/{id}/enable", content: null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DisableDeviceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync($"api/devices/{id}/disable", content: null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<TagDto>> GetDeviceTagsAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        var tags = await httpClient.GetFromJsonAsync<TagDto[]>($"api/devices/{deviceId}/tags", JsonOptions, cancellationToken);
        return tags ?? Array.Empty<TagDto>();
    }

    public async Task<TagDto?> GetTagAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<TagDto>($"api/tags/{id}", JsonOptions, cancellationToken);
    }

    public async Task<TagDto> CreateModbusTagAsync(
        CreateModbusTagRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/tags/modbus", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await ReadRequiredAsync<TagDto>(response, cancellationToken);
    }

    public async Task<TagDto> CreateSnmpTagAsync(
        CreateSnmpTagRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/tags/snmp", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await ReadRequiredAsync<TagDto>(response, cancellationToken);
    }

    public async Task EnableTagAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync($"api/tags/{id}/enable", content: null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DisableTagAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync($"api/tags/{id}/disable", content: null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task<T> ReadRequiredAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);

        if (value is null)
        {
            throw new DispatcherApiException("API вернул пустой ответ.");
        }

        return value;
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = string.IsNullOrWhiteSpace(responseBody)
            ? $"API вернул ошибку HTTP {(int)response.StatusCode}."
            : $"API вернул ошибку HTTP {(int)response.StatusCode}: {responseBody}";

        throw new DispatcherApiException(message, (int)response.StatusCode);
    }
}
