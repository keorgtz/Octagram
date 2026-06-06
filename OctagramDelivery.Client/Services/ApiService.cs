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

    public Task<HttpResponseMessage> DeleteClienteAsync(int negocioId, int id)
        => _http.DeleteAsync($"api/negocios/{negocioId}/clientes/{id}");

    public Task<HttpResponseMessage> AssignProductosClienteAsync(int negocioId, int clienteId, AssignProductsRequest req)
        => _http.PostAsJsonAsync($"api/negocios/{negocioId}/clientes/{clienteId}/productos", req);

    // ── Productos ─────────────────────────────────────────────────
    public Task<List<ProductDto>?> GetProductosAsync(int negocioId)
        => _http.GetFromJsonAsync<List<ProductDto>>($"api/negocios/{negocioId}/productos");

    public Task<HttpResponseMessage> CreateProductoAsync(int negocioId, CreateProductRequest req)
        => _http.PostAsJsonAsync($"api/negocios/{negocioId}/productos", req);

    public Task<HttpResponseMessage> UpdateProductoAsync(int negocioId, int id, CreateProductRequest req)
        => _http.PutAsJsonAsync($"api/negocios/{negocioId}/productos/{id}", req);

    public Task<HttpResponseMessage> DeleteProductoAsync(int negocioId, int id)
        => _http.DeleteAsync($"api/negocios/{negocioId}/productos/{id}");

    // ── Perfiles de precio ────────────────────────────────────────
    public Task<List<PriceTierDto>?> GetPerfilesProductoAsync(int negocioId, int productoId)
        => _http.GetFromJsonAsync<List<PriceTierDto>>($"api/negocios/{negocioId}/productos/{productoId}/perfiles");

    public Task<HttpResponseMessage> CreatePerfilAsync(int negocioId, int productoId, UpsertPriceTierRequest req)
        => _http.PostAsJsonAsync($"api/negocios/{negocioId}/productos/{productoId}/perfiles", req);

    public Task<HttpResponseMessage> UpdatePerfilAsync(int negocioId, int productoId, int id, UpsertPriceTierRequest req)
        => _http.PutAsJsonAsync($"api/negocios/{negocioId}/productos/{productoId}/perfiles/{id}", req);

    public Task<HttpResponseMessage> DeletePerfilAsync(int negocioId, int productoId, int id)
        => _http.DeleteAsync($"api/negocios/{negocioId}/productos/{productoId}/perfiles/{id}");

    // ── Usuarios por negocio ──────────────────────────────────────
    public Task<List<UsuarioNegocioDto>?> GetUsuariosNegocioAsync(int negocioId)
        => _http.GetFromJsonAsync<List<UsuarioNegocioDto>>($"api/negocios/{negocioId}/usuarios");

    public Task<HttpResponseMessage> AsignarUsuarioNegocioAsync(int negocioId, AsignarUsuarioRequest req)
        => _http.PostAsJsonAsync($"api/negocios/{negocioId}/usuarios", req);

    public Task<HttpResponseMessage> CrearUsuarioNegocioAsync(int negocioId, CrearUsuarioNegocioRequest req)
        => _http.PostAsJsonAsync($"api/negocios/{negocioId}/usuarios/nuevo", req);

    public Task<HttpResponseMessage> RemoverUsuarioNegocioAsync(int negocioId, int userId)
        => _http.DeleteAsync($"api/negocios/{negocioId}/usuarios/{userId}");

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

    // ── Historial ─────────────────────────────────────────────────
    public Task<List<JornadaResumenDto>?> GetHistorialAsync(int? negocioId = null, int? repartidorId = null, int page = 1)
    {
        var q = new System.Text.StringBuilder($"api/jornadas/historial?page={page}");
        if (negocioId.HasValue)    q.Append($"&negocioId={negocioId}");
        if (repartidorId.HasValue) q.Append($"&repartidorId={repartidorId}");
        return _http.GetFromJsonAsync<List<JornadaResumenDto>>(q.ToString());
    }

    // ── Reportes ─────────────────────────────────────────────────
    public Task<ReporteJornadaDto?> GetReporteJornadaAsync(int jornadaId)
        => _http.GetFromJsonAsync<ReporteJornadaDto>($"api/reportes/{jornadaId}");

    // ── Permisos por negocio ──────────────────────────────────────
    public Task<HttpResponseMessage> SetPermisosAsync(int negocioId, int userId, SetPermisosRequest req)
        => _http.PutAsJsonAsync($"api/negocios/{negocioId}/usuarios/{userId}/permisos", req);

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
