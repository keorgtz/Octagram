# OctagramDelivery — Plan de Implementación por Fases

**Versión:** 3.0 — Plan de lo que falta implementar  
**Fecha:** Junio 2026  
**Estado actual:** App funcional con Login, Jornada Repartidor, Dashboards y gestión de Negocios/Usuarios  
**Objetivo de este plan:** Completar la app con catálogos, mejoras de UX, historial, SignalR y deploy production-ready

---

## Diagnóstico — Qué está hecho vs. qué falta

### ✅ Ya implementado

| Módulo | Estado |
|--------|--------|
| Auth JWT (Login, token refresh, seed Admin/Admin) | ✅ |
| AppDbContext + 10 entidades EF Core + migraciones | ✅ |
| API: Auth, Negocios, Usuarios, Clientes, Productos, Jornadas, Dashboard | ✅ |
| ApiService con todos los métodos HTTP del cliente | ✅ (faltan 2 Delete) |
| JornadaHub (SignalR) | ✅ Hub existe, pero sin conexión en dashboards |
| Páginas Admin: Dashboard, Negocios, Usuarios | ✅ |
| Páginas Gerente/Supervisor: Dashboard | ✅ |
| JornadaPage (Repartidor) — hoja de cálculo | ✅ |
| MeridianUI aplicado, paleta verde, iconos Octagram | ✅ |
| Docker Compose + Dockerfiles + nginx | ✅ |

### ❌ Falta implementar

| Módulo | Fase |
|--------|------|
| `GestionProductos.razor` (Gerente/Supervisor) | Fase 1 |
| `GestionClientes.razor` (Gerente/Supervisor) | Fase 1 |
| NavMenu — links Catálogos para Gerente y Supervisor | Fase 1 |
| `ApiService.DeleteClienteAsync` y `DeleteProductoAsync` | Fase 1 |
| Gramaje presets en JornadaPage (¼KG, ½KG, 1KG) | Fase 2 |
| Historial de jornadas del Repartidor | Fase 2 |
| `HistorialJornadas.razor` (Admin, Supervisor, Gerente) | Fase 3 |
| Endpoint `GET /api/jornadas/historial` | Fase 3 |
| SignalR en DashboardSupervisor y DashboardGerente | Fase 4 |
| Deploy final en `octagram-app/api.endevour.mx` | Fase 5 |

---

## Fase 1 — Catálogos: Productos y Clientes

**Entregable:** Gerente y Supervisor pueden gestionar el catálogo de productos y clientes de su negocio desde la UI.

---

### 1.1 Contexto de negocio activo

Gerente puede tener múltiples negocios. Supervisor tiene exactamente uno (por claim JWT).  
Tanto `GestionProductos` como `GestionClientes` leen el `negocioId` de la siguiente manera:

- **Supervisor:** tomar `negocioIds[0]` del claim JWT (siempre uno solo).
- **Gerente:** mostrar un `MudSelect` para elegir negocio activo. Guardar la selección en `SessionStorage` para que persista mientras navega.

Ambas páginas comparten este patrón de selector:

```razor
@inject AuthService AuthSvc

@code {
    private List<TenantDto> _misNegocios = new();
    private int _negocioActivo;

    protected override async Task OnInitializedAsync()
    {
        var user = await AuthSvc.GetUserInfoAsync();
        if (user?.Rol == UserRole.Supervisor)
        {
            _negocioActivo = user.NegocioIds.First();
            await Cargar();
        }
        else
        {
            _misNegocios = await Api.GetNegociosAsync() ?? new();
            if (_misNegocios.Count == 1) { _negocioActivo = _misNegocios[0].Id; await Cargar(); }
        }
    }
}
```

---

### 1.2 `GestionProductos.razor`

**Archivo:** `OctagramDelivery.Client/Pages/Shared/GestionProductos.razor`  
**Rutas:** `@page "/gerente/productos"` y `@page "/supervisor/productos"`

#### Estructura visual

```
┌─ PageHeader: "PRODUCTOS" ─────────────────────────────────────────────┐
│  Negocio: [selector si es Gerente]        [+ Nuevo Producto]           │
├────────────────────────────────────────────────────────────────────────┤
│  data-card:                                                             │
│  MudTable Items="_productos"                                            │
│  ┌──────────────────┬─────────────┬───────────────┬──────┬──────────┐ │
│  │ NOMBRE           │ TIPO        │ PRECIO/UNIDAD │ EST. │ ACCIONES │ │
│  ├──────────────────┼─────────────┼───────────────┼──────┼──────────┤ │
│  │ Leche Entera     │ Pieza       │ $18.00        │ Act. │ ✏ 🗑     │ │
│  │ Queso Fresco     │ Gramaje     │ $120.00/KG    │ Act. │ ✏ 🗑     │ │
│  └──────────────────┴─────────────┴───────────────┴──────┴──────────┘ │
└────────────────────────────────────────────────────────────────────────┘
```

#### Código completo

```razor
@page "/gerente/productos"
@page "/supervisor/productos"
@attribute [Authorize(Roles = $"{nameof(UserRole.Gerente)},{nameof(UserRole.Supervisor)}")]
@inject ApiService Api
@inject AuthService AuthSvc
@inject ISnackbar Snackbar
@inject IDialogService Dialog

<PageTitle>Productos · OctagramDelivery</PageTitle>

<div class="page-content">
    <div class="page-header">
        <div>
            <h1 class="h-page-title">Productos</h1>
            <p class="page-subtitle">Catálogo de productos del negocio</p>
        </div>
        <div class="page-header-actions">
            @if (_esGerente && _misNegocios.Count > 1)
            {
                <MudSelect T="int" Value="_negocioActivo" ValueChanged="CambiarNegocio"
                           Label="Negocio" Variant="Variant.Outlined" Dense="true"
                           Style="min-width:200px;">
                    @foreach (var n in _misNegocios)
                    {
                        <MudSelectItem T="int" Value="n.Id">@n.Nombre</MudSelectItem>
                    }
                </MudSelect>
            }
            <MudButton StartIcon="@Icons.Material.Rounded.Add"
                       Variant="Variant.Filled" Color="Color.Primary"
                       OnClick="AbrirCrear" Disabled="_negocioActivo == 0">
                Nuevo Producto
            </MudButton>
        </div>
    </div>

    @if (_cargando)
    {
        <div class="loading-state"><MudProgressCircular Color="Color.Primary" Indeterminate="true" /></div>
    }
    else
    {
        <div class="data-card">
            <MudTable Items="_productos" Hover="true" Dense="true" Striped="false">
                <HeaderContent>
                    <MudTh>Nombre</MudTh>
                    <MudTh Style="text-align:center;">Tipo</MudTh>
                    <MudTh Style="text-align:right;">Precio / Unidad</MudTh>
                    <MudTh Style="text-align:center;">Estado</MudTh>
                    <MudTh Style="width:72px;text-align:center;">Acciones</MudTh>
                </HeaderContent>
                <RowTemplate>
                    <MudTd><span class="fw-600">@context.Nombre</span></MudTd>
                    <MudTd Style="text-align:center;">
                        <MudChip T="string" Size="Size.Small" Variant="Variant.Filled"
                                 Color="@(context.TipoMedida == TipoMedida.Gramaje ? Color.Info : Color.Default)">
                            @(context.TipoMedida == TipoMedida.Gramaje ? "Gramaje" : "Pieza")
                        </MudChip>
                    </MudTd>
                    <MudTd Style="text-align:right;font-variant-numeric:tabular-nums;">
                        @context.PrecioPorUnidad.ToString("C", _mx)
                        @if (context.TipoMedida == TipoMedida.Gramaje)
                        {
                            <span class="text-muted" style="font-size:11px;"> /KG</span>
                        }
                    </MudTd>
                    <MudTd Style="text-align:center;">
                        <MudChip T="string" Size="Size.Small" Variant="Variant.Filled"
                                 Color="@(context.IsActive ? Color.Success : Color.Default)">
                            @(context.IsActive ? "Activo" : "Inactivo")
                        </MudChip>
                    </MudTd>
                    <MudTd Style="text-align:center;">
                        <div style="display:inline-flex;gap:2px;align-items:center;">
                            <MudIconButton Icon="@Icons.Material.Rounded.Edit"
                                           Size="Size.Small" Color="Color.Default"
                                           OnClick="@(() => AbrirEditar(context))" />
                            <MudIconButton Icon="@Icons.Material.Rounded.Delete"
                                           Size="Size.Small" Color="Color.Default"
                                           OnClick="@(() => Eliminar(context))" />
                        </div>
                    </MudTd>
                </RowTemplate>
                <NoRecordsContent>
                    <div class="empty-state">
                        <span class="material-symbols-rounded">inventory_2</span>
                        <p>No hay productos registrados para este negocio.</p>
                    </div>
                </NoRecordsContent>
            </MudTable>
        </div>
    }
</div>

<!-- Dialog crear/editar -->
<MudDialog @bind-Visible="_dialogOpen">
    <TitleContent>
        <div style="display:flex;align-items:center;gap:10px;">
            <span class="material-symbols-rounded" style="color:var(--primary);font-size:20px;">inventory_2</span>
            <span>@(_esCrear ? "Nuevo Producto" : "Editar Producto")</span>
        </div>
    </TitleContent>
    <DialogContent>
        <MudTextField @bind-Value="_nombre" Label="Nombre del producto"
                      Variant="Variant.Outlined" FullWidth="true" Class="mb-3"
                      Adornment="Adornment.Start" AdornmentIcon="@Icons.Material.Rounded.Label" />
        <MudSelect T="TipoMedida" @bind-Value="_tipoMedida" Label="Tipo de medida"
                   Variant="Variant.Outlined" FullWidth="true" Class="mb-3">
            <MudSelectItem T="TipoMedida" Value="TipoMedida.Pieza">Pieza (unidades)</MudSelectItem>
            <MudSelectItem T="TipoMedida" Value="TipoMedida.Gramaje">Gramaje (precio por KG)</MudSelectItem>
        </MudSelect>
        <MudNumericField T="decimal" @bind-Value="_precio"
                         Label="@(_tipoMedida == TipoMedida.Gramaje ? "Precio por KG" : "Precio por pieza")"
                         Variant="Variant.Outlined" FullWidth="true" Min="0"
                         Adornment="Adornment.Start" AdornmentText="$"
                         Format="F2" Culture="_mx" />
    </DialogContent>
    <DialogActions>
        <MudButton Variant="Variant.Text" OnClick="CerrarDialog">Cancelar</MudButton>
        <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="Guardar">Guardar</MudButton>
    </DialogActions>
</MudDialog>

@code {
    private static readonly System.Globalization.CultureInfo _mx =
        System.Globalization.CultureInfo.GetCultureInfo("es-MX");

    private List<ProductDto> _productos = new();
    private List<TenantDto> _misNegocios = new();
    private bool _cargando = true;
    private bool _esGerente;
    private int _negocioActivo;

    private bool _dialogOpen, _esCrear;
    private int _editId;
    private string _nombre = "";
    private TipoMedida _tipoMedida = TipoMedida.Pieza;
    private decimal _precio;

    protected override async Task OnInitializedAsync()
    {
        var user = await AuthSvc.GetUserInfoAsync();
        _esGerente = user?.Rol == UserRole.Gerente;

        if (_esGerente)
        {
            _misNegocios = await Api.GetNegociosAsync() ?? new();
            _negocioActivo = _misNegocios.FirstOrDefault()?.Id ?? 0;
        }
        else
        {
            _negocioActivo = user?.NegocioIds.FirstOrDefault() ?? 0;
        }

        if (_negocioActivo != 0) await Cargar();
        _cargando = false;
    }

    private async Task CambiarNegocio(int id) { _negocioActivo = id; await Cargar(); }

    private async Task Cargar()
    {
        _cargando = true;
        _productos = await Api.GetProductosAsync(_negocioActivo) ?? new();
        _cargando = false;
    }

    private void AbrirCrear()
    {
        _esCrear = true; _editId = 0;
        _nombre = ""; _tipoMedida = TipoMedida.Pieza; _precio = 0;
        _dialogOpen = true;
    }

    private void AbrirEditar(ProductDto p)
    {
        _esCrear = false; _editId = p.Id;
        _nombre = p.Nombre; _tipoMedida = p.TipoMedida; _precio = p.PrecioPorUnidad;
        _dialogOpen = true;
    }

    private void CerrarDialog() => _dialogOpen = false;

    private async Task Guardar()
    {
        var req = new CreateProductRequest { Nombre = _nombre, TipoMedida = _tipoMedida, PrecioPorUnidad = _precio };
        var resp = _esCrear
            ? await Api.CreateProductoAsync(_negocioActivo, req)
            : await Api.UpdateProductoAsync(_negocioActivo, _editId, req);

        if (resp.IsSuccessStatusCode)
        {
            Snackbar.Add("Producto guardado.", Severity.Success);
            CerrarDialog(); await Cargar();
        }
        else Snackbar.Add("Error al guardar el producto.", Severity.Error);
    }

    private async Task Eliminar(ProductDto p)
    {
        var ok = await Dialog.ShowMessageBox("Desactivar producto",
            $"¿Desactivar \"{p.Nombre}\"?", yesText: "Desactivar", cancelText: "Cancelar");
        if (ok == true)
        {
            await Api.DeleteProductoAsync(_negocioActivo, p.Id);
            Snackbar.Add("Producto desactivado.", Severity.Warning);
            await Cargar();
        }
    }
}
```

---

### 1.3 `GestionClientes.razor`

**Archivo:** `OctagramDelivery.Client/Pages/Shared/GestionClientes.razor`  
**Rutas:** `@page "/gerente/clientes"` y `@page "/supervisor/clientes"`

#### Estructura visual

```
┌─ PageHeader: "CLIENTES" ──────────────────────────────────────────────┐
│  [selector negocio si Gerente]               [+ Nuevo Cliente]         │
├────────────────────────────────────────────────────────────────────────┤
│  MudTable Items="_clientes"                                             │
│  ┌──────────────┬───────────┬────────────────┬──────────┬──────────┐  │
│  │ NOMBRE       │ TELÉFONO  │ DÍAS ENTREGA   │ PRODS.   │ ACCIONES │  │
│  ├──────────────┼───────────┼────────────────┼──────────┼──────────┤  │
│  │ Juan García  │ 555-1234  │ L M X J V      │ 2 prods  │ ✏ 📦 🗑  │  │
│  └──────────────┴───────────┴────────────────┴──────────┴──────────┘  │
└────────────────────────────────────────────────────────────────────────┘
```

Los 3 botones de Acciones son:
- ✏ Editar datos del cliente
- 📦 Asignar productos (abre un segundo dialog)
- 🗑 Desactivar

#### Días de entrega — bitmask

El campo `DiasEntrega` es un `int` bitmask donde `Lun=1, Mar=2, Mie=4, Jue=8, Vie=16, Sab=32, Dom=64`. Usar checkboxes en el form:

```csharp
private static readonly (int Bit, string Label)[] _diasOptions =
{
    (1, "Lun"), (2, "Mar"), (4, "Mié"), (8, "Jue"), (16, "Vie"), (32, "Sáb"), (64, "Dom")
};
private int _diasEntrega;

private bool TieneDia(int bit) => (_diasEntrega & bit) != 0;
private void ToggleDia(int bit, bool on)
    => _diasEntrega = on ? _diasEntrega | bit : _diasEntrega & ~bit;

// En el markup:
// @foreach (var d in _diasOptions)
// {
//     <MudCheckBox T="bool" Label="@d.Label"
//                  Value="@TieneDia(d.Bit)"
//                  ValueChanged="@(v => ToggleDia(d.Bit, v))" />
// }
```

#### Dialog de asignación de productos

Abre un segundo `MudDialog` cuando el usuario pulsa el botón de productos de un cliente. Muestra todos los productos del negocio. Por cada producto, un checkbox para incluirlo, y si está incluido: campos `CantidadHabitual` y `PrecioEspecial`.

```razor
<!-- Dialog asignación de productos -->
<MudDialog @bind-Visible="_dialogProductos">
    <TitleContent>
        <div style="display:flex;align-items:center;gap:10px;">
            <span class="material-symbols-rounded" style="color:var(--primary);font-size:20px;">inventory</span>
            <span>Productos de @_clienteEditando?.Nombre</span>
        </div>
    </TitleContent>
    <DialogContent>
        @foreach (var prod in _todosLosProductos)
        {
            var asig = _asignaciones.FirstOrDefault(a => a.ProductId == prod.Id);
            var activo = asig != null;
            <div style="display:flex;align-items:center;gap:12px;padding:8px 0;border-bottom:1px solid var(--gray-line);">
                <MudCheckBox T="bool" Value="activo"
                             ValueChanged="@(v => ToggleProducto(prod.Id, v))" />
                <div style="flex:1;">
                    <div class="fw-500">@prod.Nombre</div>
                    <div style="font-size:11px;color:var(--gray-muted);">
                        @(prod.TipoMedida == TipoMedida.Gramaje ? "Gramaje" : "Pieza") —
                        @prod.PrecioPorUnidad.ToString("C", _mx)
                    </div>
                </div>
                @if (activo && asig != null)
                {
                    <MudNumericField T="decimal" Value="asig.CantidadHabitual"
                                     ValueChanged="@(v => asig.CantidadHabitual = v)"
                                     Label="Cant. habitual" Style="width:100px;"
                                     Variant="Variant.Outlined" Margin="Margin.Dense" Min="0" />
                    <MudNumericField T="decimal?" Value="asig.PrecioEspecial"
                                     ValueChanged="@(v => asig.PrecioEspecial = v)"
                                     Label="Precio especial" Style="width:110px;"
                                     Variant="Variant.Outlined" Margin="Margin.Dense" Min="0"
                                     Placeholder="(normal)" />
                }
            </div>
        }
    </DialogContent>
    <DialogActions>
        <MudButton Variant="Variant.Text" OnClick="@(() => _dialogProductos = false)">Cancelar</MudButton>
        <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="GuardarProductos">Guardar</MudButton>
    </DialogActions>
</MudDialog>
```

#### Lógica `GuardarProductos`

```csharp
private async Task GuardarProductos()
{
    var req = new AssignProductsRequest
    {
        Productos = _asignaciones.Select(a => new AssignProductItem
        {
            ProductId    = a.ProductId,
            CantidadHabitual = a.CantidadHabitual,
            PrecioEspecial   = a.PrecioEspecial
        }).ToList()
    };
    var resp = await Api.AssignProductosClienteAsync(_negocioActivo, _clienteEditando!.Id, req);
    if (resp.IsSuccessStatusCode)
    {
        Snackbar.Add("Productos del cliente actualizados.", Severity.Success);
        _dialogProductos = false;
        await Cargar();
    }
    else Snackbar.Add("Error al guardar los productos.", Severity.Error);
}
```

---

### 1.4 Métodos faltantes en `ApiService.cs`

Agregar al final de la región de Clientes y Productos:

```csharp
// En región Clientes (después de AssignProductosClienteAsync):
public Task<HttpResponseMessage> DeleteClienteAsync(int negocioId, int id)
    => _http.DeleteAsync($"api/negocios/{negocioId}/clientes/{id}");

// En región Productos (después de UpdateProductoAsync):
public Task<HttpResponseMessage> DeleteProductoAsync(int negocioId, int id)
    => _http.DeleteAsync($"api/negocios/{negocioId}/productos/{id}");
```

---

### 1.5 NavMenu — agregar Catálogos

**Archivo:** `OctagramDelivery.Client/Layout/NavMenu.razor`

Reemplazar los cases de `Gerente` y `Supervisor`:

```razor
case UserRole.Gerente:
    <div class="nav-section">
        <NavLink class="nav-item" href="/gerente/dashboard" Match="NavLinkMatch.Prefix" ActiveClass="active">
            <span class="material-symbols-rounded nav-item-icon">dashboard</span>
            @if (SidebarOpen) { <span class="nav-item-label">Vista General</span> }
        </NavLink>
    </div>
    <div class="nav-divider"></div>
    @if (SidebarOpen) { <div class="nav-category">Catálogos</div> }
    <div class="nav-section">
        <NavLink class="nav-item" href="/gerente/clientes" Match="NavLinkMatch.Prefix" ActiveClass="active">
            <span class="material-symbols-rounded nav-item-icon">people</span>
            @if (SidebarOpen) { <span class="nav-item-label">Clientes</span> }
        </NavLink>
        <NavLink class="nav-item" href="/gerente/productos" Match="NavLinkMatch.Prefix" ActiveClass="active">
            <span class="material-symbols-rounded nav-item-icon">inventory_2</span>
            @if (SidebarOpen) { <span class="nav-item-label">Productos</span> }
        </NavLink>
    </div>
    break;

case UserRole.Supervisor:
    <div class="nav-section">
        <NavLink class="nav-item" href="/supervisor/dashboard" Match="NavLinkMatch.Prefix" ActiveClass="active">
            <span class="material-symbols-rounded nav-item-icon">dashboard</span>
            @if (SidebarOpen) { <span class="nav-item-label">Dashboard</span> }
        </NavLink>
    </div>
    <div class="nav-divider"></div>
    @if (SidebarOpen) { <div class="nav-category">Catálogos</div> }
    <div class="nav-section">
        <NavLink class="nav-item" href="/supervisor/clientes" Match="NavLinkMatch.Prefix" ActiveClass="active">
            <span class="material-symbols-rounded nav-item-icon">people</span>
            @if (SidebarOpen) { <span class="nav-item-label">Clientes</span> }
        </NavLink>
        <NavLink class="nav-item" href="/supervisor/productos" Match="NavLinkMatch.Prefix" ActiveClass="active">
            <span class="material-symbols-rounded nav-item-icon">inventory_2</span>
            @if (SidebarOpen) { <span class="nav-item-label">Productos</span> }
        </NavLink>
    </div>
    break;
```

---

## Fase 2 — Mejoras a JornadaPage

**Entregable:** Presets rápidos de gramaje funcionales. Repartidor puede ver su historial.

---

### 2.1 Gramaje presets (¼KG, ½KG, 1KG)

**Archivo:** `OctagramDelivery.Client/Pages/Repartidor/JornadaPage.razor`

#### Cómo funciona

Cuando un producto es de tipo `Gramaje`, al hacer click en la celda de cantidad entregada (input) se muestra un `MudPopover` con 4 botones rápidos. Al elegir un preset, el valor se inserta en el campo y el popover se cierra. "Personalizado" solo cierra el popover y deja el input libre.

#### Variables de estado que agregar

```csharp
private bool _popoverOpen;
private int _popoverRondaId;
private int _popoverClienteId;
private int _popoverProductoId;
```

#### Markup del popover (dentro de la celda de gramaje)

```razor
@* Dentro del loop de celdas, cuando el producto es Gramaje: *@
@if (producto.TipoMedida == TipoMedida.Gramaje)
{
    <div style="position:relative;display:inline-block;">
        <input class="celda-input"
               type="number" step="0.01"
               value="@GetEntregado(rondaId, clienteId, productId)"
               @onchange="@(e => SetEntregado(rondaId, clienteId, productId, e))"
               @onfocus="@(() => AbrirPreset(rondaId, clienteId, productId))" />
        <MudPopover Open="@IsPresetAbierto(rondaId, clienteId, productId)"
                    AnchorOrigin="Origin.BottomLeft"
                    TransformOrigin="Origin.TopLeft">
            <div style="display:flex;gap:6px;padding:8px;background:var(--white);
                        border-radius:10px;box-shadow:var(--shadow-md);">
                @foreach (var preset in _gramajes)
                {
                    <button class="preset-btn" @onclick="@(() => AplicarPreset(rondaId, clienteId, productId, preset.kg))">
                        @preset.label
                    </button>
                }
                <button class="preset-btn preset-btn-custom"
                        @onclick="@(() => _popoverOpen = false)">
                    Personaliz.
                </button>
            </div>
        </MudPopover>
    </div>
}
```

#### Datos y lógica

```csharp
private static readonly (decimal kg, string label)[] _gramajes =
{
    (0.25m, "¼ KG"), (0.5m, "½ KG"), (1.0m, "1 KG")
};

private void AbrirPreset(int rondaId, int clienteId, int productoId)
{
    _popoverRondaId   = rondaId;
    _popoverClienteId = clienteId;
    _popoverProductoId = productoId;
    _popoverOpen = true;
}

private bool IsPresetAbierto(int rondaId, int clienteId, int productoId)
    => _popoverOpen && _popoverRondaId == rondaId
       && _popoverClienteId == clienteId && _popoverProductoId == productoId;

private void AplicarPreset(int rondaId, int clienteId, int productoId, decimal kg)
{
    SetEntregado(rondaId, clienteId, productoId, kg);
    _popoverOpen = false;
    QueueAutoSave();
}
```

#### CSS para los botones de preset (agregar en `app.css`)

```css
.preset-btn {
    background: var(--pr-100);
    color: var(--primary);
    border: 1px solid var(--pr-300);
    border-radius: 8px;
    font-size: 12px;
    font-weight: 600;
    padding: 5px 10px;
    cursor: pointer;
    transition: background 0.15s;
    white-space: nowrap;
}
.preset-btn:hover { background: var(--pr-200); }
.preset-btn-custom {
    background: var(--gray-foot);
    color: var(--gray-muted);
    border-color: var(--gray-line);
}
.preset-btn-custom:hover { background: var(--gray-line); }
```

---

### 2.2 Endpoint de historial de jornadas en API

**Archivo:** `OctagramDelivery.Api/Controllers/JornadasController.cs` — agregar endpoint

El historial necesita un endpoint que permita al repartidor ver sus propias jornadas pasadas, y a los supervisores/gerentes ver las del equipo.

```csharp
// GET /api/jornadas/historial?negocioId=X&repartidorId=Y&page=1&pageSize=20
[HttpGet("historial")]
[Authorize]
public async Task<ActionResult<List<JornadaResumenDto>>> GetHistorial(
    [FromQuery] int? negocioId,
    [FromQuery] string? repartidorId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var userId = User.FindFirst("UserId")?.Value;
    var rol = User.FindFirst("Rol")?.Value;

    IQueryable<DeliveryDay> query = _ctx.DeliveryDays
        .Include(j => j.AppUser)
        .Include(j => j.Tenant)
        .Where(j => j.Status != JornadaStatus.Abierta); // Solo cerradas/revisadas

    // Repartidor solo ve las suyas
    if (rol == nameof(UserRole.Repartidor))
        query = query.Where(j => j.AppUserId == userId);
    else if (negocioId.HasValue)
        query = query.Where(j => j.TenantId == negocioId.Value);

    if (!string.IsNullOrEmpty(repartidorId))
        query = query.Where(j => j.AppUserId == repartidorId);

    var total = await query.CountAsync();
    var jornadas = await query
        .OrderByDescending(j => j.Fecha)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return Ok(jornadas.Select(j => new JornadaResumenDto
    {
        Id          = j.Id,
        Fecha       = j.Fecha,
        RepartidorNombre = j.AppUser?.FullName ?? "",
        NegocioNombre    = j.Tenant?.Nombre ?? "",
        Estado      = j.Status,
        TotalNeto   = j.Rounds.SelectMany(r => r.Details)
                        .Sum(d => (d.Entregado - d.Devuelto) * d.PrecioUnitario)
    }));
}
```

**DTO a agregar en `OctagramDelivery.Application/DTOs`:**

```csharp
public class JornadaResumenDto
{
    public int      Id               { get; set; }
    public DateOnly Fecha            { get; set; }
    public string   RepartidorNombre { get; set; } = "";
    public string   NegocioNombre    { get; set; } = "";
    public JornadaStatus Estado      { get; set; }
    public decimal  TotalNeto        { get; set; }
}
```

**ApiService — agregar:**

```csharp
public Task<List<JornadaResumenDto>?> GetHistorialAsync(int? negocioId = null, string? repartidorId = null, int page = 1)
{
    var q = new List<string>();
    if (negocioId.HasValue)    q.Add($"negocioId={negocioId}");
    if (repartidorId != null)  q.Add($"repartidorId={repartidorId}");
    q.Add($"page={page}");
    return _http.GetFromJsonAsync<List<JornadaResumenDto>>($"api/jornadas/historial?{string.Join("&", q)}");
}
```

---

### 2.3 Historial del Repartidor — página

**Archivo:** `OctagramDelivery.Client/Pages/Repartidor/HistorialRepartidor.razor`  
**Ruta:** `@page "/repartidor/historial"`

```razor
@page "/repartidor/historial"
@attribute [Authorize(Roles = nameof(UserRole.Repartidor))]
@inject ApiService Api
@inject NavigationManager Nav

<PageTitle>Historial · OctagramDelivery</PageTitle>

<div class="page-content">
    <div class="page-header">
        <div>
            <h1 class="h-page-title">Historial</h1>
            <p class="page-subtitle">Mis jornadas anteriores</p>
        </div>
    </div>

    @if (_cargando)
    {
        <div class="loading-state"><MudProgressCircular Color="Color.Primary" Indeterminate="true" /></div>
    }
    else
    {
        <div class="data-card">
            <MudTable Items="_jornadas" Hover="true" Dense="true">
                <HeaderContent>
                    <MudTh>Fecha</MudTh>
                    <MudTh Style="text-align:center;">Estado</MudTh>
                    <MudTh Style="text-align:right;">Total Neto</MudTh>
                    <MudTh Style="width:60px;"></MudTh>
                </HeaderContent>
                <RowTemplate>
                    <MudTd>
                        <span class="fw-500">
                            @context.Fecha.ToString("dddd d 'de' MMMM", _mx)
                        </span>
                    </MudTd>
                    <MudTd Style="text-align:center;">
                        <MudChip T="string" Size="Size.Small"
                                 Color="@(context.Estado == JornadaStatus.Revisada ? Color.Success : Color.Default)">
                            @context.Estado
                        </MudChip>
                    </MudTd>
                    <MudTd Style="text-align:right;font-variant-numeric:tabular-nums;color:var(--vi);font-weight:700;">
                        @context.TotalNeto.ToString("C", _mx)
                    </MudTd>
                    <MudTd>
                        <MudIconButton Icon="@Icons.Material.Rounded.ChevronRight"
                                       Size="Size.Small"
                                       OnClick="@(() => Nav.NavigateTo($"/repartidor/jornada/{context.Id}"))" />
                    </MudTd>
                </RowTemplate>
                <NoRecordsContent>
                    <div class="empty-state">
                        <span class="material-symbols-rounded">history</span>
                        <p>No tienes jornadas anteriores.</p>
                    </div>
                </NoRecordsContent>
            </MudTable>
        </div>
    }
</div>

@code {
    private static readonly System.Globalization.CultureInfo _mx =
        System.Globalization.CultureInfo.GetCultureInfo("es-MX");

    private List<JornadaResumenDto> _jornadas = new();
    private bool _cargando = true;

    protected override async Task OnInitializedAsync()
    {
        _jornadas = await Api.GetHistorialAsync() ?? new();
        _cargando = false;
    }
}
```

**NavMenu del Repartidor:** Agregar link de historial en el layout de repartidor (`MainLayout.razor`), en la barra superior:

```razor
<NavLink href="/repartidor/historial" class="icon-btn" title="Historial">
    <span class="material-symbols-rounded">history</span>
</NavLink>
```

---

## Fase 3 — Historial y Reportes para Supervisores

**Entregable:** Supervisor y Admin pueden ver jornadas pasadas del equipo y exportarlas a CSV.

---

### 3.1 `HistorialJornadas.razor`

**Archivo:** `OctagramDelivery.Client/Pages/Shared/HistorialJornadas.razor`  
**Rutas:** `@page "/admin/historial"` y `@page "/supervisor/historial"` y `@page "/gerente/historial"`

```razor
@page "/admin/historial"
@page "/supervisor/historial"
@page "/gerente/historial"
@attribute [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Gerente)},{nameof(UserRole.Supervisor)}")]
@inject ApiService Api
@inject AuthService AuthSvc
@inject ISnackbar Snackbar
@inject IJSRuntime JS

<PageTitle>Historial · OctagramDelivery</PageTitle>

<div class="page-content">
    <div class="page-header">
        <div>
            <h1 class="h-page-title">Historial de Jornadas</h1>
            <p class="page-subtitle">Consulta de jornadas cerradas del equipo</p>
        </div>
        <div class="page-header-actions">
            <MudDatePicker @bind-Date="_fechaDesde" Label="Desde" Variant="Variant.Outlined"
                           Margin="Margin.Dense" DateFormat="dd/MM/yyyy" />
            <MudDatePicker @bind-Date="_fechaHasta" Label="Hasta" Variant="Variant.Outlined"
                           Margin="Margin.Dense" DateFormat="dd/MM/yyyy" />
            <MudButton StartIcon="@Icons.Material.Rounded.Search"
                       Variant="Variant.Outlined" Color="Color.Primary"
                       OnClick="Buscar">
                Buscar
            </MudButton>
            <MudIconButton Icon="@Icons.Material.Rounded.Download"
                           Color="Color.Default" title="Exportar CSV"
                           OnClick="ExportarCsv" Disabled="_jornadas.Count == 0" />
        </div>
    </div>

    @if (_cargando)
    {
        <div class="loading-state"><MudProgressCircular Color="Color.Primary" Indeterminate="true" /></div>
    }
    else
    {
        <div class="data-card">
            <MudTable @ref="_tabla" Items="_jornadas" Hover="true" Dense="true"
                      Striped="false" RowsPerPage="25">
                <HeaderContent>
                    <MudTh>Fecha</MudTh>
                    <MudTh>Repartidor</MudTh>
                    <MudTh>Negocio</MudTh>
                    <MudTh Style="text-align:center;">Estado</MudTh>
                    <MudTh Style="text-align:right;">Total Neto</MudTh>
                    <MudTh Style="width:48px;"></MudTh>
                </HeaderContent>
                <RowTemplate>
                    <MudTd>@context.Fecha.ToString("dd/MM/yyyy")</MudTd>
                    <MudTd><span class="fw-500">@context.RepartidorNombre</span></MudTd>
                    <MudTd><span class="text-muted">@context.NegocioNombre</span></MudTd>
                    <MudTd Style="text-align:center;">
                        <MudChip T="string" Size="Size.Small"
                                 Color="@(context.Estado == JornadaStatus.Revisada ? Color.Success : Color.Default)">
                            @context.Estado
                        </MudChip>
                    </MudTd>
                    <MudTd Style="text-align:right;font-variant-numeric:tabular-nums;
                                  color:var(--vi);font-weight:700;">
                        @context.TotalNeto.ToString("C", _mx)
                    </MudTd>
                    <MudTd>
                        <MudIconButton Icon="@Icons.Material.Rounded.Visibility"
                                       Size="Size.Small" Color="Color.Default"
                                       OnClick="@(() => VerDetalle(context.Id))" />
                    </MudTd>
                </RowTemplate>
                <NoRecordsContent>
                    <div class="empty-state">
                        <span class="material-symbols-rounded">history</span>
                        <p>No hay jornadas en el período seleccionado.</p>
                    </div>
                </NoRecordsContent>
                <PagerContent>
                    <MudTablePager />
                </PagerContent>
            </MudTable>
        </div>
    }
</div>

@code {
    private static readonly System.Globalization.CultureInfo _mx =
        System.Globalization.CultureInfo.GetCultureInfo("es-MX");

    private MudTable<JornadaResumenDto>? _tabla;
    private List<JornadaResumenDto> _jornadas = new();
    private bool _cargando;
    private DateTime? _fechaDesde = DateTime.Today.AddDays(-30);
    private DateTime? _fechaHasta = DateTime.Today;
    private int _negocioActivo;

    protected override async Task OnInitializedAsync()
    {
        var user = await AuthSvc.GetUserInfoAsync();
        _negocioActivo = user?.NegocioIds.FirstOrDefault() ?? 0;
        await Buscar();
    }

    private async Task Buscar()
    {
        _cargando = true;
        _jornadas = await Api.GetHistorialAsync(negocioId: _negocioActivo) ?? new();
        // Filtrar por fechas en cliente (o añadir params al endpoint)
        if (_fechaDesde.HasValue)
            _jornadas = _jornadas.Where(j => j.Fecha >= DateOnly.FromDateTime(_fechaDesde.Value)).ToList();
        if (_fechaHasta.HasValue)
            _jornadas = _jornadas.Where(j => j.Fecha <= DateOnly.FromDateTime(_fechaHasta.Value)).ToList();
        _cargando = false;
    }

    private async Task VerDetalle(int jornadaId)
    {
        // Navegar a vista de detalle read-only
        // La JornadaPage ya soporta ?readOnly=true o una ruta /supervisor/jornada/{id}
        // Por ahora abrir en tab: aún pendiente si se quiere modal o página
        await JS.InvokeVoidAsync("open", $"/supervisor/jornada/{jornadaId}", "_blank");
    }

    private async Task ExportarCsv()
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Fecha,Repartidor,Negocio,Estado,Total Neto");
        foreach (var j in _jornadas)
            csv.AppendLine($"{j.Fecha:dd/MM/yyyy},{j.RepartidorNombre},{j.NegocioNombre},{j.Estado},{j.TotalNeto:F2}");

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        var b64   = Convert.ToBase64String(bytes);
        await JS.InvokeVoidAsync("eval",
            $"const a=document.createElement('a');a.href='data:text/csv;base64,{b64}';" +
            $"a.download='historial_{DateTime.Today:yyyyMMdd}.csv';a.click()");
    }
}
```

**NavMenu — agregar para Admin, Gerente y Supervisor:**

```razor
// Admin: agregar después de Usuarios
<NavLink class="nav-item" href="/admin/historial" Match="NavLinkMatch.Prefix" ActiveClass="active">
    <span class="material-symbols-rounded nav-item-icon">history</span>
    @if (SidebarOpen) { <span class="nav-item-label">Historial</span> }
</NavLink>

// Gerente y Supervisor: agregar en sección de Catálogos o nueva sección Reportes
<NavLink class="nav-item" href="/[rol]/historial" Match="NavLinkMatch.Prefix" ActiveClass="active">
    <span class="material-symbols-rounded nav-item-icon">history</span>
    @if (SidebarOpen) { <span class="nav-item-label">Historial</span> }
</NavLink>
```

---

## Fase 4 — SignalR en Dashboards (Tiempo Real)

**Entregable:** Supervisores y Gerentes ven actualizaciones automáticas cuando un repartidor guarda su jornada.

---

### 4.1 Verificar que JornadaHub emite eventos

**Archivo:** `OctagramDelivery.Api/Hubs/JornadaHub.cs`

El Hub ya existe. El `JornadasController` (bulk save endpoint) debe emitir el evento después de guardar:

```csharp
// En JornadasController.cs — inyectar IHubContext
private readonly IHubContext<JornadaHub> _hub;

// Después de _ctx.SaveChangesAsync() en BulkSaveDetalles:
await _hub.Clients.Group($"negocio-{jornada.TenantId}")
    .SendAsync("JornadaActualizada", jornada.TenantId);
```

Verificar que el Hub tiene el método `JoinNegocio`:

```csharp
public class JornadaHub : Hub
{
    public async Task JoinNegocio(int negocioId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"negocio-{negocioId}");

    public async Task LeaveNegocio(int negocioId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"negocio-{negocioId}");
}
```

---

### 4.2 Cliente SignalR en Blazor WASM

**NuGet requerido en OctagramDelivery.Client:**

```xml
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="10.0.*" />
```

**Servicio base reutilizable:** `OctagramDelivery.Client/Services/HubService.cs`

```csharp
using Microsoft.AspNetCore.SignalR.Client;

namespace OctagramDelivery.Client.Services;

public class HubService : IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly AuthService _auth;
    private readonly string _hubUrl;

    public HubService(AuthService auth, string hubUrl)
    {
        _auth = auth;
        _hubUrl = hubUrl;
    }

    public async Task ConnectAsync()
    {
        var token = await _auth.GetTokenAsync();
        _connection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options => options.AccessTokenProvider = () => Task.FromResult(token)!)
            .WithAutomaticReconnect()
            .Build();
        await _connection.StartAsync();
    }

    public async Task JoinNegocioAsync(int negocioId)
    {
        if (_connection?.State == HubConnectionState.Connected)
            await _connection.InvokeAsync("JoinNegocio", negocioId);
    }

    public IDisposable On(string method, Action<int> handler)
        => _connection!.On(method, handler);

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
            await _connection.DisposeAsync();
    }
}
```

**Registro en `Program.cs`:**

```csharp
// Registrar como transient porque cada página maneja su propia conexión
builder.Services.AddTransient<HubService>(sp =>
    new HubService(
        sp.GetRequiredService<AuthService>(),
        builder.HostEnvironment.BaseAddress.TrimEnd('/') + "/hubs/jornada"
    ));
```

---

### 4.3 Integrar SignalR en `DashboardSupervisor.razor`

Modificaciones al `@code` del componente existente:

```csharp
@inject HubService Hub

// Variables adicionales:
private bool _enVivo = false;
private IDisposable? _subscription;

// Al final de OnInitializedAsync, después de cargar los datos:
try
{
    await Hub.ConnectAsync();
    await Hub.JoinNegocioAsync(_data!.NegocioId);
    _subscription = Hub.On("JornadaActualizada", async (int id) =>
    {
        await InvokeAsync(async () =>
        {
            await Cargar(_fechaSeleccionada);
            StateHasChanged();
        });
    });
    _enVivo = true;
}
catch { /* SignalR opcional — app funciona sin él */ }

// Implementar IAsyncDisposable para limpiar la conexión:
public async ValueTask DisposeAsync()
{
    _subscription?.Dispose();
    await Hub.DisposeAsync();
}
```

**Badge "En vivo" en el header del dashboard:**

```razor
@if (_enVivo)
{
    <div style="display:flex;align-items:center;gap:6px;font-size:11px;
                font-weight:600;color:var(--em);padding:4px 10px;
                background:var(--em-bg);border-radius:20px;">
        <span style="width:7px;height:7px;border-radius:50%;
                     background:var(--em);display:inline-block;
                     animation:pulse 2s infinite;"></span>
        En vivo
    </div>
}
```

**CSS para el pulso (agregar en app.css):**

```css
@keyframes pulse {
    0%, 100% { opacity: 1; }
    50%       { opacity: 0.4; }
}
```

---

### 4.4 Integrar SignalR en `DashboardGerente.razor`

Mismo patrón que Supervisor, pero unirse a un grupo por cada negocio asignado:

```csharp
foreach (var negocio in _data!)
    await Hub.JoinNegocioAsync(negocio.NegocioId);
```

---

## Fase 5 — Deploy en producción

**Entregable:** Stack completo corriendo en Ubuntu Server, accesible desde `octagram-app.endevour.mx` y `octagram-api.endevour.mx` con SSL automático vía Cloudflare.

---

### 5.1 Estructura de archivos en el servidor

```
/root/docker/apps/octagram/
├── docker-compose.yml
├── .env                          ← NO subir a git
├── api/
│   ├── Dockerfile
│   └── (todo el código fuente o bind mount)
└── client/
    ├── Dockerfile
    └── nginx.conf
```

La forma más práctica para este proyecto es hacer `git clone` del repositorio en `/root/docker/apps/octagram/` y que los Dockerfiles hagan la compilación dentro del contenedor (multi-stage build). Así solo se necesita `git pull` + `docker compose up --build` para actualizar.

---

### 5.2 Dockerfile — API

**Archivo:** `OctagramDelivery.Api/Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["OctagramDelivery.Api/OctagramDelivery.Api.csproj", "OctagramDelivery.Api/"]
COPY ["OctagramDelivery.Application/OctagramDelivery.Application.csproj", "OctagramDelivery.Application/"]
COPY ["OctagramDelivery.Domain/OctagramDelivery.Domain.csproj", "OctagramDelivery.Domain/"]
COPY ["OctagramDelivery.Infrastructure/OctagramDelivery.Infrastructure.csproj", "OctagramDelivery.Infrastructure/"]
RUN dotnet restore "OctagramDelivery.Api/OctagramDelivery.Api.csproj"
COPY . .
RUN dotnet publish "OctagramDelivery.Api/OctagramDelivery.Api.csproj" \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OctagramDelivery.Api.dll"]
```

---

### 5.3 Dockerfile — Cliente Blazor WASM

**Archivo:** `OctagramDelivery.Client/Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["OctagramDelivery.Client/OctagramDelivery.Client.csproj", "OctagramDelivery.Client/"]
COPY ["OctagramDelivery.Application/OctagramDelivery.Application.csproj", "OctagramDelivery.Application/"]
COPY ["OctagramDelivery.Domain/OctagramDelivery.Domain.csproj", "OctagramDelivery.Domain/"]
RUN dotnet restore "OctagramDelivery.Client/OctagramDelivery.Client.csproj"
COPY . .
RUN dotnet publish "OctagramDelivery.Client/OctagramDelivery.Client.csproj" \
    -c Release -o /app/publish --no-restore

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY OctagramDelivery.Client/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
```

---

### 5.4 nginx.conf — Cliente

**Archivo:** `OctagramDelivery.Client/nginx.conf`

Este nginx hace dos cosas: sirve el WASM/PWA y hace reverse proxy de `/api/` y `/hubs/` al contenedor de la API, eliminando cualquier problema de CORS en producción.

```nginx
events { worker_connections 1024; }

http {
    include       /etc/nginx/mime.types;
    default_type  application/octet-stream;
    sendfile on;

    server {
        listen 80;
        root /usr/share/nginx/html;
        index index.html;

        # SPA — todas las rutas del cliente
        location / {
            try_files $uri $uri/ /index.html;
        }

        # Proxy a la API (elimina CORS)
        location /api/ {
            proxy_pass         http://octagram-api:8080/api/;
            proxy_http_version 1.1;
            proxy_set_header   Host              $host;
            proxy_set_header   X-Real-IP         $remote_addr;
            proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
            proxy_set_header   X-Forwarded-Proto $scheme;
        }

        # Proxy SignalR WebSocket
        location /hubs/ {
            proxy_pass          http://octagram-api:8080/hubs/;
            proxy_http_version  1.1;
            proxy_set_header    Upgrade    $http_upgrade;
            proxy_set_header    Connection "upgrade";
            proxy_set_header    Host       $host;
            proxy_cache_bypass  $http_upgrade;
        }

        # Cache agresivo para assets compilados
        location ~* \.(wasm|js|css|png|svg|ico|json)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }
}
```

---

### 5.5 `docker-compose.yml` — Stack completo

**Archivo:** `/root/docker/apps/octagram/docker-compose.yml`

```yaml
version: "3.9"

services:

  octagram-db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: octagram-db
    restart: unless-stopped
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "${DB_PASSWORD}"
      MSSQL_PID: "Express"
    volumes:
      - octagram-db-data:/var/opt/mssql
    networks:
      - octagram-internal
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "$$SA_PASSWORD" -Q "SELECT 1" -C
      interval: 10s
      timeout: 5s
      retries: 10

  octagram-api:
    build:
      context: /root/docker/apps/octagram
      dockerfile: OctagramDelivery.Api/Dockerfile
    container_name: octagram-api
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: "http://+:8080"
      ConnectionStrings__DefaultConnection: >
        Server=octagram-db;Database=OctagramDelivery;
        User Id=sa;Password=${DB_PASSWORD};
        TrustServerCertificate=True;
      Jwt__SecretKey: "${JWT_SECRET}"
      Jwt__Issuer: "octagram-api.endevour.mx"
      Jwt__Audience: "octagram-app.endevour.mx"
    expose:
      - "8080"
    depends_on:
      octagram-db:
        condition: service_healthy
    networks:
      - octagram-internal
      - proxy

  octagram-app:
    build:
      context: /root/docker/apps/octagram
      dockerfile: OctagramDelivery.Client/Dockerfile
    container_name: octagram-app
    restart: unless-stopped
    expose:
      - "80"
    depends_on:
      - octagram-api
    networks:
      - proxy

volumes:
  octagram-db-data:
    name: octagram-db-data

networks:
  octagram-internal:
    driver: bridge
    name: octagram-internal
  proxy:
    external: true
    name: proxy
```

> **Nota:** La red `proxy` debe ser la misma red externa que usa tu Nginx Proxy Manager. Verificar el nombre exacto con `docker network ls`.

---

### 5.6 `.env` — Variables de entorno

**Archivo:** `/root/docker/apps/octagram/.env` (NO subir a Git — agregar a `.gitignore`)

```env
# Base de datos SQL Server
DB_PASSWORD=TuPasswordSeguroSQLServer2026!

# JWT — mínimo 32 caracteres, alfanumérico + símbolos
JWT_SECRET=OctagramJWT_Secret_Muy_Largo_Y_Seguro_256bits_2026!
```

---

### 5.7 Nginx Proxy Manager — Proxy Hosts

Crear dos entradas en el panel web de NPM (`http://TU-IP:81`):

| Campo                     | octagram-app               | octagram-api               |
|---------------------------|----------------------------|----------------------------|
| Domain Names              | `octagram-app.endevour.mx` | `octagram-api.endevour.mx` |
| Forward Hostname / IP     | `octagram-app`             | `octagram-api`             |
| Forward Port              | `80`                       | `8080`                     |
| Block Common Exploits     | ✅                         | ✅                         |
| Force SSL                 | ✅                         | ✅                         |
| SSL Certificate           | Let's Encrypt (auto)       | Let's Encrypt (auto)       |
| HTTP/2 Support            | ✅                         | ✅                         |
| WebSockets Support        | ✅ (para SignalR)          | ✅                         |

**Nota importante sobre WebSockets para SignalR:** En la configuración avanzada del proxy host de `octagram-app`, agregar el header personalizado:

```nginx
# En "Custom Nginx Configuration" del proxy host de octagram-app:
proxy_read_timeout 3600s;
proxy_send_timeout 3600s;
```

---

### 5.8 Cloudflare Tunnel — actualizar `config.yml`

**Archivo:** `/root/docker/cloudflared/config.yml`

Agregar las dos entradas de Octagram antes de la regla catch-all `http_status:404`:

```yaml
tunnel: TU_TUNNEL_ID
credentials-file: /etc/cloudflared/TU_TUNNEL_ID.json

ingress:
  # Subdominios existentes del servidor...
  - hostname: endevour.mx
    service: http://npm:80

  # OctagramDelivery
  - hostname: octagram-app.endevour.mx
    service: http://npm:80

  - hostname: octagram-api.endevour.mx
    service: http://npm:80

  # Catch-all obligatorio al final
  - service: http_status:404
```

Reiniciar cloudflared después del cambio:

```bash
docker compose restart cloudflared
```

---

### 5.9 DNS en Cloudflare — registros CNAME

En el panel de Cloudflare, en el dominio `endevour.mx`, crear:

| Tipo  | Nombre          | Destino                          | Proxy  |
|-------|-----------------|----------------------------------|--------|
| CNAME | `octagram-app`  | `TU_TUNNEL_ID.cfargotunnel.com`  | ✅ ON  |
| CNAME | `octagram-api`  | `TU_TUNNEL_ID.cfargotunnel.com`  | ✅ ON  |

---

### 5.10 Comandos de despliegue

#### Primera vez

```bash
# Clonar el repositorio en el servidor
cd /root/docker/apps
git clone https://github.com/tu-usuario/OctagramDelivery.git octagram
cd octagram

# Crear el .env con contraseñas reales
nano .env

# Levantar todo el stack
docker compose up -d --build

# Verificar que todos los contenedores están corriendo
docker compose ps

# Ver logs de la API (debe mostrar migraciones aplicadas y servidor escuchando)
docker logs octagram-api -f
```

#### Actualización de código

```bash
cd /root/docker/apps/octagram
git pull

# Rebuild y restart solo de los contenedores que cambiaron
docker compose up -d --build octagram-api
# o si el cliente también cambió:
docker compose up -d --build
```

#### Rollback si algo falla

```bash
# Ver los últimos commits
git log --oneline -10

# Volver a un commit anterior
git checkout COMMIT_HASH

# Rebuild
docker compose up -d --build
```

#### Backup de la base de datos

```bash
# Crear backup
docker exec octagram-db /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U SA -P "$DB_PASSWORD" \
    -Q "BACKUP DATABASE OctagramDelivery TO DISK='/tmp/backup.bak'"

# Copiar backup al host
docker cp octagram-db:/tmp/backup.bak /root/backups/octagram_$(date +%Y%m%d).bak
```

---

## Secuencia de implementación recomendada

| Orden | Tarea | Tiempo estimado |
|-------|-------|-----------------|
| 1 | Agregar `DeleteClienteAsync` y `DeleteProductoAsync` en `ApiService.cs` | 5 min |
| 2 | Crear `GestionProductos.razor` | 1 hora |
| 3 | Crear `GestionClientes.razor` (con dialog de productos) | 2 horas |
| 4 | Actualizar `NavMenu.razor` con links de Catálogos | 15 min |
| 5 | Agregar endpoint `GET /api/jornadas/historial` + DTO | 30 min |
| 6 | Agregar `GetHistorialAsync` en `ApiService.cs` | 5 min |
| 7 | Crear `HistorialRepartidor.razor` | 30 min |
| 8 | Crear `HistorialJornadas.razor` (Admin/Sup/Gerente) | 1 hora |
| 9 | Gramaje presets en `JornadaPage.razor` | 45 min |
| 10 | Instalar SignalR Client NuGet, crear `HubService.cs` | 20 min |
| 11 | Integrar SignalR en `DashboardSupervisor.razor` | 30 min |
| 12 | Integrar SignalR en `DashboardGerente.razor` | 20 min |
| 13 | Verificar Dockerfiles y `nginx.conf` están actualizados | 15 min |
| 14 | Crear entradas en Nginx Proxy Manager + DNS Cloudflare | 20 min |
| 15 | Deploy en servidor: `git clone` + `.env` + `docker compose up` | 30 min |

**Total estimado:** ~8 horas de implementación

---

## Convenciones que mantener

- Todos los imports de `ISnackbar`, `IDialogService` y `ApiService` se inyectan con `@inject`.
- `_cargando` controla el spinner; nunca mostrar tabla vacía mientras carga.
- `CultureInfo "es-MX"` como campo estático `_mx` en cada componente.
- Botones de acción en tablas: siempre `Color.Default`, envueltos en `display:inline-flex;gap:2px`.
- Empty states: siempre `<div class="empty-state">` con `material-symbols-rounded` + `<p>`.
- Los `MudDialog` usan `@bind-Visible` y se cierran seteando `_dialogOpen = false`.
- Soft deletes: nunca `DELETE` físico — llamar al endpoint que pone `IsActive = false`.

---

*OctagramDelivery — Plan v3.0 · KeorSoft · Junio 2026*
