using Blazored.LocalStorage;
using OctagramDelivery.Application.DTOs;
using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Client.Services;

// ── Modelos locales ─────────────────────────────────────────────────────────

public class JornadaSnapshot
{
    public JornadaDto Jornada { get; set; } = new();
    public List<ProductDto> Productos { get; set; } = new();
    public List<CustomerDto> Clientes { get; set; } = new();
    public DateTime GuardadoEn { get; set; } = DateTime.UtcNow;
}

/// <summary>Snapshot completo del negocio: productos, clientes, grupos y secciones.</summary>
public class NegocioSnapshot
{
    public List<ProductDto> Productos { get; set; } = new();
    public List<CustomerDto> Clientes { get; set; } = new();
    public List<GrupoProductoDto> GruposProducto { get; set; } = new();
    public List<SeccionDto> Secciones { get; set; } = new();
    public DateTime SincronizadoEn { get; set; } = DateTime.UtcNow;
}

public enum PendingOpType { BulkSaveRonda, ToggleExclusion }

public class PendingOperation
{
    public string OpId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public PendingOpType Tipo { get; set; }
    public int JornadaId { get; set; }
    public int NegocioId { get; set; }
    // BulkSaveRonda
    public int RondaId { get; set; }
    public List<DetalleUpsertItem> Detalles { get; set; } = new();
    // ToggleExclusion
    public int ClienteId { get; set; }
    public bool ExcluidoDeEfectivo { get; set; }
    public MetodoPago MetodoPago { get; set; }
    // Metadata
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public int Intentos { get; set; }
}

// ── Servicio ────────────────────────────────────────────────────────────────

public class LocalDataService
{
    private readonly ILocalStorageService _storage;

    private static string SnapKey(int negocioId) => $"tc_j_{negocioId}";
    private static string NegocioKey(int negocioId) => $"tc_neg_{negocioId}";
    private const string OpsKey = "tc_ops";

    public LocalDataService(ILocalStorageService storage) => _storage = storage;

    // ── Snapshot de jornada ───────────────────────────────────────────────

    public async Task<JornadaSnapshot?> GetSnapshotAsync(int negocioId)
    {
        try { return await _storage.GetItemAsync<JornadaSnapshot>(SnapKey(negocioId)); }
        catch { return null; }
    }

    public async Task SaveSnapshotAsync(int negocioId, JornadaSnapshot snap)
    {
        try { await _storage.SetItemAsync(SnapKey(negocioId), snap); }
        catch { }
    }

    // ── Snapshot de negocio (datos maestros) ─────────────────────────────

    public async Task<NegocioSnapshot?> GetNegocioSnapshotAsync(int negocioId)
    {
        try { return await _storage.GetItemAsync<NegocioSnapshot>(NegocioKey(negocioId)); }
        catch { return null; }
    }

    public async Task SaveNegocioSnapshotAsync(int negocioId, NegocioSnapshot snap)
    {
        try { await _storage.SetItemAsync(NegocioKey(negocioId), snap); }
        catch { }
    }

    // ── Operaciones pendientes ────────────────────────────────────────────

    public async Task<List<PendingOperation>> GetOpsAsync()
    {
        try { return await _storage.GetItemAsync<List<PendingOperation>>(OpsKey) ?? new(); }
        catch { return new(); }
    }

    /// <summary>Agrega o reemplaza la op del mismo tipo/clave para evitar duplicados.</summary>
    public async Task UpsertOpAsync(PendingOperation op)
    {
        var ops = await GetOpsAsync();
        if (op.Tipo == PendingOpType.BulkSaveRonda)
            ops.RemoveAll(o => o.Tipo == PendingOpType.BulkSaveRonda && o.RondaId == op.RondaId);
        else if (op.Tipo == PendingOpType.ToggleExclusion)
            ops.RemoveAll(o => o.Tipo == PendingOpType.ToggleExclusion
                            && o.JornadaId == op.JornadaId && o.ClienteId == op.ClienteId);
        ops.Add(op);
        try { await _storage.SetItemAsync(OpsKey, ops); } catch { }
    }

    public async Task RemoveOpAsync(string opId)
    {
        var ops = await GetOpsAsync();
        ops.RemoveAll(o => o.OpId == opId);
        try
        {
            if (ops.Count == 0) await _storage.RemoveItemAsync(OpsKey);
            else await _storage.SetItemAsync(OpsKey, ops);
        }
        catch { }
    }

    public async Task<int> CountPendingAsync() => (await GetOpsAsync()).Count;
}
