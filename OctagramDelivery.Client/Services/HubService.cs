using Microsoft.AspNetCore.SignalR.Client;

namespace OctagramDelivery.Client.Services;

public class HubService : IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly AuthService _auth;
    private readonly string _hubUrl;

    public HubService(AuthService auth, string hubUrl)
    {
        _auth = auth;
        _hubUrl = hubUrl;
    }

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public async Task ConnectAsync()
    {
        var token = await _auth.GetTokenAsync();
        _connection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .WithAutomaticReconnect()
            .Build();

        await _connection.StartAsync();
    }

    public async Task JoinNegocioAsync(int negocioId)
    {
        if (IsConnected)
            await _connection!.InvokeAsync("JoinNegocioGroup", negocioId);
    }

    public async Task LeaveNegocioAsync(int negocioId)
    {
        if (IsConnected)
            await _connection!.InvokeAsync("LeaveNegocioGroup", negocioId);
    }

    public IDisposable On<T>(string method, Action<T> handler)
        => _connection!.On(method, handler);

    public IDisposable OnVoid(string method, Action handler)
        => _connection!.On(method, handler);

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
            await _connection.DisposeAsync();
    }
}
