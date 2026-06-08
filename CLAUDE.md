# OctagramDelivery — Reglas del proyecto

## Regla obligatoria: Control de permisos en cada vista

**Toda página Blazor (`.razor` con `@page`) debe sin excepción incluir las dos capas de control de acceso:**

### Capa 1 — Rol (atributo de ruta)
```razor
@attribute [Authorize(Roles = $"{nameof(UserRole.X)},{nameof(UserRole.Y)}")]
```
Nunca omitir. Si una vista es accesible para todos los autenticados, usar `@attribute [Authorize]` sin Roles.

### Capa 2 — Permiso granular (código en `OnInitializedAsync`)
Para vistas que aceptan el rol `Supervisor`, verificar el permiso correspondiente:

```csharp
var user = await AuthSvc.GetCurrentUser();
if (user?.Rol == UserRole.Supervisor && negocioId > 0 && !user.TienePermiso(Permiso.XYZ, negocioId))
{
    Nav.NavigateTo("/");
    return;
}
```

Los helpers disponibles en `UserInfo`:
- `user.TienePermiso(Permiso.Flag, negocioId)` — chequeo genérico
- `user.PuedeVerHojaReparto(negocioId)` — shortcut para `VerHojaReparto`

### Permisos disponibles (`Permiso` enum, flags)
| Flag | Descripción | Quién lo necesita |
|------|-------------|-------------------|
| `VerDashboard` | Acceso al dashboard | Supervisor |
| `VerReportes` | Ver reportes de jornada | Supervisor |
| `GestionClientes` | Crear/editar clientes | Supervisor |
| `GestionProductos` | Crear/editar productos | Supervisor |
| `VerHojaReparto` | Ver y monitorear la hoja de reparto | Supervisor |

Admin y Gerente **siempre** tienen acceso completo; no necesitan permisos adicionales.
Repartidor **solo** accede a la hoja de reparto propia; no aplica el sistema de permisos de negocio.

### Patrón de vista unificada
La hoja de reparto (`JornadaPage`, `ClienteJornadaPage`) es **una sola vista** para todos los roles:
- Repartidor → edición completa, ruta propia `/repartidor/jornada`
- Gerente/Admin monitoreando → mismo componente vía `/gerente/jornada/{id}`, solo lectura
- Supervisor con `VerHojaReparto` → mismo componente vía `/gerente/jornada/{id}`, solo lectura

No crear vistas duplicadas por rol. Usar `_soloLectura`, `_puedeAbrir` y el parámetro `JornadaId` para adaptar el comportamiento dentro del mismo componente.

---

## Rutas canónicas (NO duplicar por rol)

**Regla:** Una sola ruta por función. Si la vista necesita comportarse distinto por rol, se adapta internamente con flags, NO creando una nueva página.

| Ruta | Roles | Componente |
|------|-------|------------|
| `/dashboard` | Admin, Gerente, Supervisor | `Pages/Dashboard.razor` |
| `/catalogos/clientes` | Admin, Gerente, Supervisor | `Pages/Gerente/GestionClientes.razor` |
| `/catalogos/productos` | Admin, Gerente, Supervisor | `Pages/Gerente/GestionProductos.razor` |
| `/catalogos/grupos` | Admin, Gerente, Supervisor | `Pages/Gerente/GestionGrupos.razor` |
| `/gerente/jornadas` | Admin, Gerente, Supervisor | `Pages/Gerente/JornadasActivasPage.razor` |
| `/gerente/jornada/{JornadaId:int}` | Admin, Gerente, Supervisor | `Pages/Repartidor/JornadaPage.razor` (solo lectura) |
| `/repartidor/jornada` | Repartidor | `Pages/Repartidor/JornadaPage.razor` (edición) |
| `/jornada/{JornadaId:int}/cliente/{ClienteId:int}` | Admin, Gerente, Supervisor | `Pages/Repartidor/ClienteJornadaPage.razor` |
| `/repartidor/jornada/cliente/{ClienteId:int}` | Repartidor | `Pages/Repartidor/ClienteJornadaPage.razor` |

Routes **eliminadas** (no recrear): `/admin/dashboard`, `/gerente/dashboard`, `/supervisor/dashboard`, `/gerente/clientes`, `/supervisor/clientes`, `/gerente/productos`, `/gerente/grupos`.

---

## Corrección de timezone: siempre pasar fecha local del cliente

El servidor corre en UTC. El cliente (Blazor WASM) tiene la hora local del navegador.
**NUNCA** usar `DateTime.Today` o `DateTime.UtcNow` en el servidor para determinar "hoy" — siempre recibir la fecha del cliente.

### Patrón en API (controllers):
```csharp
// Query param con fecha local del cliente
public async Task<IActionResult> GetHoy([FromQuery] DateOnly? fecha = null)
{
    var hoy = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow); // fallback seguro
    ...
}

// En OpenJornadaRequest:
public DateOnly? LocalFecha { get; set; } // siempre usar esto en lugar de DateTime.Today del server
```

### Patrón en Cliente (ApiService):
```csharp
// Siempre pasar la fecha local del cliente
var f = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");
return _http.GetFromJsonAsync<T>($"api/endpoint?fecha={f}");

// Para requests POST:
new OpenJornadaRequest { LocalFecha = DateOnly.FromDateTime(DateTime.Today) }
```

---

## SignalR — HubService

`HubService` en `OctagramDelivery.Client/Services/HubService.cs`:
- `ConnectAsync()` — conectar al hub
- `JoinNegocioAsync(int negocioId)` — unirse al grupo del negocio
- `LeaveNegocioAsync(int negocioId)` — salir del grupo
- `On<T>(string event, Func<T, Task> handler)` — suscribirse a un evento, devuelve `IDisposable`
- `DisposeAsync()` — limpiar conexión

Eventos del hub: `JornadaAbierta`, `JornadaCerrada`, `JornadaActualizada`

Grupos: `negocio-{id}`

Patrón estándar de uso en páginas:
```csharp
@implements IAsyncDisposable
@inject HubService Hub

private List<IDisposable> _hubSubs = new();

private async Task ConectarHub()
{
    try
    {
        await Hub.ConnectAsync();
        await Hub.JoinNegocioAsync(_negocioId);
        _hubSubs.Add(Hub.On<object>("JornadaActualizada", async _ =>
            await InvokeAsync(async () => { await Cargar(); StateHasChanged(); })));
        _enVivo = true; StateHasChanged();
    }
    catch { /* funciona sin SignalR */ }
}

public async ValueTask DisposeAsync()
{
    foreach (var s in _hubSubs) s.Dispose();
    await Hub.DisposeAsync();
}
```

---

## Sistema de permisos por negocio (Supervisor)

El `Permiso` enum es `[Flags]` en `OctagramDelivery.Domain/Enums/Enums.cs`:
```csharp
[Flags]
public enum Permiso
{
    Ninguno          = 0,
    VerDashboard     = 1 << 0,   // 1
    VerReportes      = 1 << 1,   // 2
    GestionClientes  = 1 << 2,   // 4
    GestionProductos = 1 << 3,   // 8
    VerHojaReparto   = 1 << 4,   // 16
    Todo             = 31
}
```

Los permisos del Supervisor se guardan como claim `"permisos"` en el JWT, codificados como `"{negocioId}:{flags}"` por negocio. El helper `UserInfo.TienePermiso(Permiso, negocioId)` decodifica esto en el cliente.

Los permisos se configuran desde la UI admin en `Pages/Admin/NegocioDetailDialog.razor`.

---

## DTOs de Dashboard

```csharp
// Application/DTOs/DashboardDtos.cs
public class DashboardRepartidorDto {
    public int UserId, JornadaId, ClientesAtendidos, TotalClientes;
    public string FullName;
    public JornadaEstado Estado;
    public decimal TotalEntregado, TotalDevuelto, TotalNeto, TotalEfectivo;
}
public class DashboardNegocioDto {
    public int TenantId; public string TenantNombre;
    public int RepartidoresActivos, RepartidoresCerrados;
    public decimal TotalEfectivoEnCalle, TotalNeto;
    public List<DashboardRepartidorDto> Repartidores;
}
public class DashboardAdminDto {
    public int TotalNegocios, TotalRepartidoresActivos;
    public decimal TotalEfectivoGlobal;
    public List<DashboardNegocioDto> Negocios;
}
```

Endpoints en `ApiService`:
- `GetDashboardAdminAsync(DateOnly? fecha)` → `DashboardAdminDto?`
- `GetDashboardGerenteAsync(DateOnly? fecha)` → `List<DashboardNegocioDto>?`
- `GetDashboardSupervisorAsync(int negocioId, DateOnly? fecha)` → `DashboardNegocioDto?`
