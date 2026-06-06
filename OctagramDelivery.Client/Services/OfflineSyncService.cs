using Blazored.LocalStorage;
using Microsoft.JSInterop;
using OctagramDelivery.Application.DTOs;

namespace OctagramDelivery.Client.Services;

public class PendingSave
{
    public int RondaId { get; set; }
    public List<DetalleUpsertItem> Detalles { get; set; } = new();
}

public class OfflineSyncService : IAsyncDisposable
{
    private readonly ILocalStorageService _storage;
    private readonly ApiService _api;
    private readonly IJSRuntime _js;
    private DotNetObjectReference<OfflineSyncService>? _jsRef;
    private bool _initialized;
    private const string QueueKey = "tc_pending_saves";

    public bool IsOnline { get; private set; } = true;
    public event Action? OnConnectivityChanged;
    public event Action? OnSynced;

    public OfflineSyncService(ILocalStorageService storage, ApiService api, IJSRuntime js)
    {
        _storage = storage;
        _api = api;
        _js = js;
    }

    public async Task InitAsync()
    {
        if (_initialized) return;
        _initialized = true;
        try
        {
            IsOnline = await _js.InvokeAsync<bool>("offlineSync.isOnline");
            _jsRef = DotNetObjectReference.Create(this);
            await _js.InvokeVoidAsync("offlineSync.init", _jsRef);
            if (IsOnline) await ProcessQueueAsync();
        }
        catch { }
    }

    [JSInvokable]
    public async Task NotifyOnline()
    {
        IsOnline = true;
        OnConnectivityChanged?.Invoke();
        await ProcessQueueAsync();
    }

    [JSInvokable]
    public void NotifyOffline()
    {
        IsOnline = false;
        OnConnectivityChanged?.Invoke();
    }

    public async Task EnqueueSaveAsync(int rondaId, List<DetalleUpsertItem> detalles)
    {
        var queue = await SafeGetQueue();
        var existing = queue.FirstOrDefault(q => q.RondaId == rondaId);
        if (existing != null) existing.Detalles = detalles;
        else queue.Add(new PendingSave { RondaId = rondaId, Detalles = detalles });
        await _storage.SetItemAsync(QueueKey, queue);
    }

    public async Task ProcessQueueAsync()
    {
        var queue = await SafeGetQueue();
        if (queue.Count == 0) return;
        var synced = new List<int>();
        foreach (var item in queue)
        {
            try
            {
                var resp = await _api.BulkSaveDetallesAsync(item.RondaId,
                    new BulkSaveRondaRequest { Detalles = item.Detalles });
                if (resp.IsSuccessStatusCode) synced.Add(item.RondaId);
            }
            catch { }
        }
        if (synced.Count > 0)
        {
            queue.RemoveAll(q => synced.Contains(q.RondaId));
            if (queue.Count == 0) await _storage.RemoveItemAsync(QueueKey);
            else await _storage.SetItemAsync(QueueKey, queue);
            OnSynced?.Invoke();
        }
    }

    public async Task<int> PendingCountAsync()
    {
        var q = await SafeGetQueue();
        return q.Count;
    }

    private async Task<List<PendingSave>> SafeGetQueue()
    {
        try { return await _storage.GetItemAsync<List<PendingSave>>(QueueKey) ?? new(); }
        catch { return new(); }
    }

    public async ValueTask DisposeAsync()
    {
        _jsRef?.Dispose();
        await ValueTask.CompletedTask;
    }
}
