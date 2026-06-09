using Microsoft.JSInterop;
using OctagramDelivery.Application.DTOs;

namespace OctagramDelivery.Client.Services;

public class OfflineSyncService : IAsyncDisposable
{
    private readonly LocalDataService _local;
    private readonly ApiService _api;
    private readonly IJSRuntime _js;
    private DotNetObjectReference<OfflineSyncService>? _jsRef;
    private System.Threading.Timer? _syncTimer;
    private bool _initialized;
    private bool _syncingNegocio;

    public bool IsOnline { get; private set; } = true;
    public event Action? OnConnectivityChanged;
    public event Action? OnNegocioSynced;
    public event Action? OnJornadaSyncNeeded;

    public OfflineSyncService(LocalDataService local, ApiService api, IJSRuntime js)
    {
        _local = local;
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
        }
        catch { }
    }

    [JSInvokable]
    public Task NotifyOnline()
    {
        IsOnline = true;
        OnConnectivityChanged?.Invoke();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public void NotifyOffline()
    {
        IsOnline = false;
        OnConnectivityChanged?.Invoke();
    }

    // ── Sincronización datos maestros del negocio ─────────────────────────

    public async Task<NegocioSnapshot?> SyncNegocioDataAsync(int negocioId)
    {
        if (negocioId == 0 || !IsOnline || _syncingNegocio) return null;
        _syncingNegocio = true;
        try
        {
            var productos = await _api.GetProductosAsync(negocioId) ?? new();
            var clientes = await _api.GetClientesAsync(negocioId) ?? new();
            var grupos = await _api.GetGruposProductoAsync(negocioId) ?? new();
            var secciones = await _api.GetSeccionesAsync(negocioId) ?? new();

            var snap = new NegocioSnapshot
            {
                Productos = productos,
                Clientes = clientes,
                GruposProducto = grupos,
                Secciones = secciones
            };
            await _local.SaveNegocioSnapshotAsync(negocioId, snap);
            OnNegocioSynced?.Invoke();
            return snap;
        }
        catch { return null; }
        finally { _syncingNegocio = false; }
    }

    public void StartPeriodicSync(int negocioId)
    {
        _syncTimer?.Dispose();
        _syncTimer = new System.Threading.Timer(
            async _ =>
            {
                if (IsOnline)
                {
                    await SyncNegocioDataAsync(negocioId);
                    OnJornadaSyncNeeded?.Invoke();
                }
            },
            null,
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5));
    }

    public async ValueTask DisposeAsync()
    {
        _syncTimer?.Dispose();
        _jsRef?.Dispose();
        await ValueTask.CompletedTask;
    }
}
