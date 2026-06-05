using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace OctagramDelivery.Client.Services;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _storage;
    private readonly HttpClient _http;

    public JwtAuthStateProvider(ILocalStorageService storage, HttpClient http)
    {
        _storage = storage;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _storage.GetItemAsStringAsync("authToken");
        if (string.IsNullOrWhiteSpace(token))
            return Anonymous();

        var claims = JwtParser.ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task NotifyLogin(string token)
    {
        await _storage.SetItemAsStringAsync("authToken", token);
        var claims = JwtParser.ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public async Task NotifyLogout()
    {
        await _storage.RemoveItemAsync("authToken");
        _http.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous()));
    }

    private static AuthenticationState Anonymous()
        => new(new ClaimsPrincipal(new ClaimsIdentity()));
}
