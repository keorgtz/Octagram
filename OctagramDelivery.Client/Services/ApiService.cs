using System.Net.Http.Json;
using OctagramDelivery.Application.DTOs;

namespace OctagramDelivery.Client.Services;

public class ApiService
{
    private readonly HttpClient _http;
    public ApiService(HttpClient http) => _http = http;

    // ── Negocios ──────────────────────────────────────────────────
    public Task<List<TenantDto>?> GetNegociosAsync()
        => _http.GetFromJsonAsync<List<TenantDto>>("api/negocios");

    public Task<HttpResponseMessage> CreateNegocioAsync(CreateTenantRequest req)
        => _http.PostAsJsonAsync("api/negocios", req);

    public Task<HttpResponseMessage> UpdateNegocioAsync(int id, CreateTenantRequest req)
        => _http.PutAsJsonAsync($"api/negocios/{id}", req);

    public Task<HttpResponseMessage> DeleteNegocioAsync(int id)
        => _http.DeleteAsync($"api/negocios/{id}");

    // ── Usuarios ──────────────────────────────────────────────────
    public Task<List<UserDto>?> GetUsuariosAsync()
        => _http.GetFromJsonAsync<List<UserDto>>("api/usuarios");

    public Task<HttpResponseMessage> CreateUsuarioAsync(CreateUserRequest req)
        => _http.PostAsJsonAsync("api/usuarios", req);

    public Task<HttpResponseMessage> UpdateUsuarioAsync(int id, UpdateUserRequest req)
        => _http.PutAsJsonAsync($"api/usuarios/{id}", req);

    public Task<HttpResponseMessage> DeleteUsuarioAsync(int id)
        => _http.DeleteAsync($"api/usuarios/{id}");

    // ── Clientes ──────────────────────────────────────────────────
    public Task<List<CustomerDto>?> GetClientesAsync(int negocioId)
        => _http.GetFromJsonAsync<List<CustomerDto>>($"api/negocios/{negocioId}/clientes");

    public Task<HttpResponseMessage> CreateClienteAsync(int negocioId, CreateCustomerRequest req)
        => _http.PostAsJsonAsync($"api/negocios/{negocioId}/clientes", req);

    public Task<HttpResponseMessage> UpdateClienteAsync(int negocioId, int id, CreateCustomerRequest req)
        => _http.PutAsJsonAsync($"api/negocios/{negocioId}/clientes/{id}", req);

    public Task<HttpResponseMessage> AssignProductosClienteAsync(int negocioId, int clienteId, AssignProductsRequest req)
        => _http.PostAsJsonAsync($"api/negocios/{negocioId}/clientes/{clienteId}/productos", req);

    // ── Productos ─────────────────────────────────────────────────
    public Task<List<ProductDto>?> GetProductosAsync(int negocioId)
        => _http.GetFromJsonAsync<List<ProductDto>>($"api/negocios/{negocioId}/productos");

    public Task<HttpResponseMessage> CreateProductoAsync(int negocioId, CreateProductRequest req)
        => _http.PostAsJsonAsync($"api/negocios/{negocioId}/productos", req);

    public Task<HttpResponseMessage> UpdateProductoAsync(int negocioId, int id, CreateProductRequest req)
        => _http.PutAsJsonAsync($"api/negocios/{negocioId}/productos/{id}", req);

    // ── Jornadas ──────────────────────────────────────────────────
    public Task<JornadaDto?> GetJornadaHoyAsync(int negocioId)
        => _http.GetFromJsonAsync<JornadaDto>($"api/jornadas/hoy?negocioId={negocioId}");

    public Task<JornadaDto?> GetJornadaByIdAsync(int id)
        => _http.GetFromJsonAsync<JornadaDto>($"api/jornadas/{id}");

    public Task<HttpResponseMessage> AbrirJornadaAsync(OpenJornadaRequest req)
        => _http.PostAsJsonAsync("api/jornadas/abrir", req);

    public Task<HttpResponseMessage> CerrarJornadaAsync(int id)
        => _http.PostAsync($"api/jornadas/{id}/cerrar", null);

    public Task<HttpResponseMessage> AddRondaAsync(int jornadaId, AddRondaRequest req)
        => _http.PostAsJsonAsync($"api/jornadas/{jornadaId}/rondas", req);

    public Task<HttpResponseMessage> DeleteRondaAsync(int rondaId)
        => _http.DeleteAsync($"api/jornadas/rondas/{rondaId}");

    public Task<HttpResponseMessage> BulkSaveDetallesAsync(int rondaId, BulkSaveRondaRequest req)
        => _http.PutAsJsonAsync($"api/jornadas/rondas/{rondaId}/detalles", req);

    public Task<HttpResponseMessage> ToggleExclusionAsync(int jornadaId, int clienteId, ToggleExclusionRequest req)
        => _http.PutAsJsonAsync($"api/jornadas/{jornadaId}/clientes/{clienteId}/exclusion", req);

    public Task<TotalesJornada?> GetTotalesAsync(int jornadaId)
        => _http.GetFromJsonAsync<TotalesJornada>($"api/jornadas/{jornadaId}/totales");

    // ── Dashboard ─────────────────────────────────────────────────
    public Task<DashboardNegocioDto?> GetDashboardSupervisorAsync(int negocioId, DateOnly? fecha = null)
    {
        var f = fecha.HasValue ? $"&fecha={fecha:yyyy-MM-dd}" : "";
        return _http.GetFromJsonAsync<DashboardNegocioDto>($"api/dashboard/supervisor?negocioId={negocioId}{f}");
    }

    public Task<List<DashboardNegocioDto>?> GetDashboardGerenteAsync(DateOnly? fecha = null)
    {
        var f = fecha.HasValue ? $"?fecha={fecha:yyyy-MM-dd}" : "";
        return _http.GetFromJsonAsync<List<DashboardNegocioDto>>($"api/dashboard/gerente{f}");
    }

    public Task<DashboardAdminDto?> GetDashboardAdminAsync(DateOnly? fecha = null)
    {
        var f = fecha.HasValue ? $"?fecha={fecha:yyyy-MM-dd}" : "";
        return _http.GetFromJsonAsync<DashboardAdminDto>($"api/dashboard/admin{f}");
    }
}
