using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using OctagramDelivery.Client;
using OctagramDelivery.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// En producción (Docker+Nginx) no hay ApiBaseUrl configurada → usa el mismo origen
// que el cliente; nginx hace el proxy interno a octagram-api:8080.
// En desarrollo apunta directo a la API local.
var configured = builder.Configuration["ApiBaseUrl"];
var apiBaseUrl = !string.IsNullOrEmpty(configured)
    ? configured
    : builder.HostEnvironment.BaseAddress;

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(p => p.GetRequiredService<JwtAuthStateProvider>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<MigrationService>();
builder.Services.AddTransient(sp => new HubService(
    sp.GetRequiredService<AuthService>(),
    apiBaseUrl.TrimEnd('/') + "/hubs/jornada"
));

await builder.Build().RunAsync();
