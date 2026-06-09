using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using OctagramDelivery.Application.DTOs;
using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly JwtAuthStateProvider _provider;
    private readonly AuthenticationStateProvider _authState;

    public AuthService(HttpClient http, JwtAuthStateProvider provider, AuthenticationStateProvider authState)
    {
        _http = http;
        _provider = provider;
        _authState = authState;
    }

    public async Task<(bool Ok, string? Error)> Login(LoginRequest request)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/auth/login", request);
            if (!resp.IsSuccessStatusCode)
            {
                var raw = await resp.Content.ReadAsStringAsync();
                var msg = raw.Trim('"');
                // Si el cuerpo está vacío o es un JSON técnico de error del servidor,
                // mostramos un mensaje legible en lugar de texto vacío o JSON crudo.
                if (string.IsNullOrWhiteSpace(msg) || msg.TrimStart().StartsWith('{'))
                    msg = resp.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        ? "Usuario o contraseña incorrectos."
                        : "Error en el servidor. Revisa los logs de la API.";
                return (false, msg);
            }
            var result = await resp.Content.ReadFromJsonAsync<LoginResponse>();
            if (result == null) return (false, "Respuesta inválida del servidor.");
            await _provider.NotifyLogin(result.Token);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task Logout() => await _provider.NotifyLogout();

    public Task<string?> GetTokenAsync()
        => _provider.GetTokenAsync();

    public async Task<UserInfo?> GetCurrentUser()
    {
        var state = await _authState.GetAuthenticationStateAsync();
        var user = state.User;
        if (!user.Identity?.IsAuthenticated ?? true) return null;

        var rolStr = user.FindFirst(ClaimTypes.Role)?.Value ?? "";
        Enum.TryParse<UserRole>(rolStr, out var rol);

        var permisosNegocio = new Dictionary<int, Permiso>();
        var permisosJson = user.FindFirst("permisos")?.Value;
        if (!string.IsNullOrEmpty(permisosJson))
        {
            try
            {
                var raw = JsonSerializer.Deserialize<Dictionary<string, int>>(permisosJson);
                if (raw != null)
                    foreach (var kv in raw)
                        if (int.TryParse(kv.Key, out var nid))
                            permisosNegocio[nid] = (Permiso)kv.Value;
            }
            catch { }
        }

        return new UserInfo
        {
            Id = int.Parse(user.FindFirst("id")?.Value ?? "0"),
            FullName = user.FindFirst("fullName")?.Value ?? "",
            Rol = rol,
            NegocioIds = user.FindAll("negocioId").Select(c => int.Parse(c.Value)).ToList(),
            PermisosNegocio = permisosNegocio
        };
    }
}

public class UserInfo
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public UserRole Rol { get; set; }
    public List<int> NegocioIds { get; set; } = new();
    public Dictionary<int, Permiso> PermisosNegocio { get; set; } = new();

    public bool TienePermiso(Permiso permiso, int negocioId)
        => PermisosNegocio.TryGetValue(negocioId, out var flags) && flags.HasFlag(permiso);

    // Admin y Gerente siempre pueden; Supervisor solo si tiene el permiso explícito.
    public bool PuedeVerHojaReparto(int negocioId)
        => Rol != UserRole.Supervisor || TienePermiso(Permiso.VerHojaReparto, negocioId);

    // Admin siempre puede; los demás roles necesitan el permiso explícito.
    public bool PuedeVerConciliacion(int negocioId)
        => Rol == UserRole.Admin || TienePermiso(Permiso.VerConciliacion, negocioId);
}
