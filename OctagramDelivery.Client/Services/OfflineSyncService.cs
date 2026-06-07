using Microsoft.JSInterop;
using OctagramDelivery.Application.DTOs;
using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Client.Services;

public class OfflineSyncService : IAsyncDisposable
{
    private readonly LocalDataService _local;
    private readonly ApiService _api;
    private readonly IJSRuntime _js;
    private DotNetObjectReference<OfflineSyncService>? _jsRef;
    private System.Threading.Timer? _syncTimer;
    private bool _initialized;
    private bool _syncing;
    private bool _syncingNegocio;

    public bool IsOnline { get; private set; } = true;
    public event Action? OnConnectivityChanged;
    public event Action? OnSynced;
    public event Action? OnNegocioSynced;

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

    // ── Sincronización datos maestros del negocio ─────────────────────────

    /// <summary>Descarga productos, clientes, grupos y secciones y los guarda en local.</summary>
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

    /// <summary>Inicia la sincronización periódica del negocio (cada 5 minutos mientras esté online).</summary>
    public void StartPeriodicSync(int negocioId)
    {
        _syncTimer?.Dispose();
        _syncTimer = new System.Threading.Timer(
            async _ => { if (IsOnline) await SyncNegocioDataAsync(negocioId); },
            null,
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5));
    }

    // ── Encolar operaciones de jornada ────────────────────────────────────

    public async Task EnqueueRondaSaveAsync(int rondaId, int jornadaId, int negocioId, List<DetalleUpsertItem> detalles)
    {
        await _local.UpsertOpAsync(new PendingOperation
        {
            Tipo      = PendingOpType.BulkSaveRonda,
            RondaId   = rondaId,
            JornadaId = jornadaId,
            NegocioId = negocioId,
            Detalles  = detalles
        });
        if (IsOnline) _ = ProcessQueueAsync();
    }

    public async Task EnqueueExclusionAsync(int jornadaId, int negocioId, int clienteId, bool excluido, MetodoPago metodo)
    {
        await _local.UpsertOpAsync(new PendingOperation
        {
            Tipo               = PendingOpType.ToggleExclusion,
            JornadaId          = jornadaId,
            NegocioId          = negocioId,
            ClienteId          = clienteId,
            ExcluidoDeEfectivo = excluido,
            MetodoPago         = metodo
        });
        if (IsOnline) _ = ProcessQueueAsync();
    }

    // ── Procesar cola de operaciones pendientes ───────────────────────────

    public async Task ProcessQueueAsync()
    {
        if (_syncing) return;
        _syncing = true;
        try
        {
            var ops = await _local.GetOpsAsync();
            if (ops.Count == 0) return;

            var synced = new List<string>();
            foreach (var op in ops)
            {
                try
                {
                    bool ok = false;
                    switch (op.Tipo)
                    {
                        case PendingOpType.BulkSaveRonda:
                            var r = await _api.BulkSaveDetallesAsync(op.RondaId,
                                new BulkSaveRondaRequest { Detalles = op.Detalles });
                            ok = r.IsSuccessStatusCode;
                            break;
                        case PendingOpType.ToggleExclusion:
                            var e = await _api.ToggleExclusionAsync(op.JornadaId, op.ClienteId,
                                new ToggleExclusionRequest
                                {
                                    ExcluidoDeEfectivo    = op.ExcluidoDeEfectivo,
                                    MetodoPagoAlternativo = op.MetodoPago
                                });
                            ok = e.IsSuccessStatusCode;
                            break;
                    }
                    if (ok) synced.Add(op.OpId);
                    else op.Intentos++;
                }
                catch { op.Intentos++; }
            }

            if (synced.Count > 0)
            {
                foreach (var id in synced) await _local.RemoveOpAsync(id);
                OnSynced?.Invoke();
            }
        }
        finally { _syncing = false; }
    }

    public Task<int> PendingCountAsync() => _local.CountPendingAsync();

    public async ValueTask DisposeAsync()
    {
        _syncTimer?.Dispose();
        _jsRef?.Dispose();
        await ValueTask.CompletedTask;
    }
}
