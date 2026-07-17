using Dispatcher.Web.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Dispatcher.Web.Services;

public sealed class TagValueRealtimeClient : IAsyncDisposable
{
    private readonly DispatcherApiClient apiClient;
    private HubConnection? connection;

    public TagValueRealtimeClient(DispatcherApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public event Func<TagValueDto, Task>? TagValueUpdated;

    public event Func<IReadOnlyList<TagValueDto>, Task>? CurrentValuesSnapshot;

    public string HubUrl => new Uri(apiClient.ApiBaseAddress, "hubs/tag-values").ToString();

    public HubConnectionState State => connection?.State ?? HubConnectionState.Disconnected;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnectionCreated();

        if (connection is null)
        {
            throw new InvalidOperationException("SignalR connection was not created.");
        }

        if (connection.State == HubConnectionState.Disconnected)
        {
            await connection.StartAsync(cancellationToken);
        }

        await connection.InvokeAsync("SubscribeToAll", cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (connection is null || connection.State == HubConnectionState.Disconnected)
        {
            return;
        }

        try
        {
            await connection.InvokeAsync("UnsubscribeFromAll", cancellationToken);
        }
        finally
        {
            await connection.StopAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (connection is null)
        {
            return;
        }

        await connection.DisposeAsync();
    }

    private void EnsureConnectionCreated()
    {
        if (connection is not null)
        {
            return;
        }

        connection = new HubConnectionBuilder()
            .WithUrl(HubUrl)
            .WithAutomaticReconnect()
            .Build();

        connection.On<TagValueDto>("TagValueUpdated", async value =>
        {
            if (TagValueUpdated is not null)
            {
                await TagValueUpdated.Invoke(value);
            }
        });

        connection.On<TagValueDto[]>("CurrentValuesSnapshot", async values =>
        {
            if (CurrentValuesSnapshot is not null)
            {
                await CurrentValuesSnapshot.Invoke(values);
            }
        });
    }
}
