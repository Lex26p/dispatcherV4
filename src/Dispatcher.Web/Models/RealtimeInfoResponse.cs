using System.Text.Json.Serialization;

namespace Dispatcher.Web.Models;

public sealed class RealtimeInfoResponse
{
    [JsonPropertyName("tagValuesHub")]
    public string? TagValuesHub { get; init; }

    [JsonPropertyName("groups")]
    public RealtimeGroupsResponse? Groups { get; init; }

    [JsonPropertyName("hubMethods")]
    public RealtimeHubMethodsResponse? HubMethods { get; init; }

    [JsonPropertyName("clientEvents")]
    public RealtimeClientEventsResponse? ClientEvents { get; init; }
}

public sealed class RealtimeGroupsResponse
{
    [JsonPropertyName("allTagValues")]
    public string? AllTagValues { get; init; }

    [JsonPropertyName("tagSpecificFormat")]
    public string? TagSpecificFormat { get; init; }
}

public sealed class RealtimeHubMethodsResponse
{
    [JsonPropertyName("subscribeToAll")]
    public string? SubscribeToAll { get; init; }

    [JsonPropertyName("unsubscribeFromAll")]
    public string? UnsubscribeFromAll { get; init; }

    [JsonPropertyName("subscribeToTag")]
    public string? SubscribeToTag { get; init; }

    [JsonPropertyName("unsubscribeFromTag")]
    public string? UnsubscribeFromTag { get; init; }
}

public sealed class RealtimeClientEventsResponse
{
    [JsonPropertyName("tagValueUpdated")]
    public string? TagValueUpdated { get; init; }

    [JsonPropertyName("currentValuesSnapshot")]
    public string? CurrentValuesSnapshot { get; init; }
}
