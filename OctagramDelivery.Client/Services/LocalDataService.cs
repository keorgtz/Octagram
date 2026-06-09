using Blazored.LocalStorage;
using OctagramDelivery.Application.DTOs;

namespace OctagramDelivery.Client.Services;

// ── Modelos locales ─────────────────────────────────────────────────────────

/// <summary>Snapshot del negocio: productos, clientes, grupos y secciones (cache para carga rápida).</summary>
public class NegocioSnapshot
{
    public List<ProductDto> Productos { get; set; } = new();
    public List<CustomerDto> Clientes { get; set; } = new();
    public List<GrupoProductoDto> GruposProducto { get; set; } = new();
    public List<SeccionDto> Secciones { get; set; } = new();
    public DateTime SincronizadoEn { get; set; } = DateTime.UtcNow;
}

// ── Servicio ────────────────────────────────────────────────────────────────

public class LocalDataService
{
    private readonly ILocalStorageService _storage;

    private static string NegocioKey(int negocioId) => $"tc_neg_{negocioId}";

    public LocalDataService(ILocalStorageService storage) => _storage = storage;

    // ── Snapshot de negocio (datos maestros, cache) ──────────────────────

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
}
