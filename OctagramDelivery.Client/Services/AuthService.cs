using System.Net.Http.Json;
using System.Security.Claims;
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
                var err = await resp.Content.ReadAsStringAsync();
                return (false, err.Trim('"'));
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

    public async Task<UserInfo?> GetCurrentUser()
    {
        var state = await _authState.GetAuthenticationStateAsync();
        var user = state.User;
        if (!user.Identity?.IsAuthenticated ?? true) return null;

        var rolStr = user.FindFirst(ClaimTypes.Role)?.Value ?? "";
        Enum.TryParse<UserRole>(rolStr, out var rol);

        return new UserInfo
        {
            Id = int.Parse(user.FindFirst("id")?.Value ?? "0"),
            FullName = user.FindFirst("fullName")?.Value ?? "",
            Rol = rol,
            NegocioIds = user.FindAll("negocioId").Select(c => int.Parse(c.Value)).ToList()
        };
    }
}

public class UserInfo
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public UserRole Rol { get; set; }
    public List<int> NegocioIds { get; set; } = new();
}
