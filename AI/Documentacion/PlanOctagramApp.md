# Plan de Desarrollo: OctagramDelivery

**Versión del Plan:** 2.0 — Plan completo por fases  
**Fecha:** Junio 2026  
**Objetivo:** PWA web para gestión y supervisión de repartidores multi-negocio, con cálculos en tiempo real, roles jerárquicos y despliegue en Ubuntu Server vía Docker.

**Dominios:**
- `octagram-app.endevour.mx` — Blazor PWA (cliente)
- `octagram-api.endevour.mx` — ASP.NET Core API

---

## Stack tecnológico

| Capa           | Tecnología                                                                 |
|----------------|----------------------------------------------------------------------------|
| Frontend       | Blazor WebAssembly (.NET 9) configurado como PWA                           |
| UI Framework   | **MudBlazor** como base de componentes + **MeridianUI** como sistema de diseño (CSS custom tokens sobrepuestos) |
| Backend        | ASP.NET Core 9 Web API + SignalR (tiempo real en dashboards)               |
| Base de datos  | SQL Server 2022 (EF Core 9, Code First)                                    |
| Auth           | JWT + Refresh Tokens (ASP.NET Core Identity como base de usuarios)         |
| Infraestructura| Ubuntu Server + Docker Compose + Nginx Proxy Manager + Cloudflare Tunnel   |
| CI/CD          | GitHub → `git pull` + `docker compose up --build` en servidor              |

### Estrategia MudBlazor + MeridianUI

MudBlazor provee la estructura de componentes (Grid, Dialog, DataTable, Snackbar, etc.).  
MeridianUI sobrescribe los tokens CSS de MudBlazor para que la apariencia sea 100% MeridianUI:

- `colors_and_type.css` se carga **antes** que el CSS de MudBlazor.
- Las variables CSS de MudBlazor (`--mud-palette-primary`, etc.) se reasignan a los tokens MeridianUI (`--primary`, `--em`, etc.) en un archivo `meridian-mud-overrides.css`.
- Se conservan `<MIcon>`, `<KpiCard>`, `<Shell>` y `<PageHeader>` del UI kit propio.
- MudBlazor se usa principalmente para: `MudDataGrid`, `MudDialog`, `MudSnackbar`, `MudNumericField`, `MudCheckBox`, `MudSelect`, `MudDrawer`.

---

## Fase 0: Preparación del entorno y estructura del proyecto

**Entregable:** Solución en Visual Studio lista, repositorio Git inicializado, estructura de proyectos definida.

### 0.1 Estructura de la solución (.sln)

```
OctagramDelivery.sln
│
├── OctagramDelivery.API/           ← ASP.NET Core 9 Web API
├── OctagramDelivery.Client/        ← Blazor WebAssembly PWA
├── OctagramDelivery.Shared/        ← DTOs, Enums, Contratos compartidos
└── OctagramDelivery.Domain/        ← Entidades de dominio + lógica de negocio
```

### 0.2 NuGet packages clave por proyecto

**API:**
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.AspNetCore.SignalR`
- `AutoMapper.Extensions.Microsoft.DependencyInjection`

**Client (Blazor WASM):**
- `MudBlazor`
- `Blazored.LocalStorage`
- `Microsoft.AspNetCore.Components.WebAssembly.Authentication`

**Shared:**
- Sin dependencias pesadas. Solo anotaciones de datos (`System.ComponentModel.DataAnnotations`).

### 0.3 Estructura de carpetas del Client

```
OctagramDelivery.Client/
├── wwwroot/
│   ├── css/
│   │   ├── meridian/
│   │   │   ├── colors_and_type.css       ← tokens MeridianUI
│   │   │   └── meridian-mud-overrides.css ← reasignación de tokens MudBlazor
│   │   └── app.css
│   ├── icons/                            ← PWA icons (192x192, 512x512)
│   ├── manifest.webmanifest
│   └── service-worker.js
├── Components/
│   ├── MeridianUI/                       ← MIcon, KpiCard, Shell, PageHeader
│   ├── Shared/                           ← NavMenu, AppBar, componentes globales
│   └── Layout/
├── Pages/
│   ├── Auth/
│   ├── Repartidor/
│   ├── Supervisor/
│   ├── Gerente/
│   └── Admin/
└── Services/                             ← HttpClient wrappers, AuthService, etc.
```

---

## Fase 1: Modelado de base de datos (Domain + EF Core)

**Entregable:** Modelo de datos completo, migraciones generadas, base de datos inicializada con seed de roles y usuario Admin.

### 1.1 Diagrama de entidades

```
Negocio ─────< Sucursal ─────< Usuario (por negocio)
                    │
                    ├────< ClienteNegocio ─────< ClienteProducto
                    │             │
                    │             └────── DiaEntrega (Lun-Dom flags)
                    │
                    └────< Producto
                    
Usuario ─────< JornadaReparto
                    │
                    ├────< RondaReparto (dinámica: 1, 2, 3...)
                    │           │
                    │           └────< DetalleRonda
                    │                    (ClienteNegocioId, ProductoId,
                    │                     CantidadEntregada, CantidadDevuelta,
                    │                     EsGramaje, GramajeKg)
                    │
                    └────< ClienteJornada (snapshot del cliente en esa jornada)
                                (ExcluidoDeEfectivo: bool, MetodoPagoAlternativo)
```

### 1.2 Tablas principales

#### `Negocios`
| Campo            | Tipo          | Notas                          |
|------------------|---------------|--------------------------------|
| Id               | Guid (PK)     |                                |
| Nombre           | nvarchar(200) |                                |
| LogoUrl          | nvarchar(500) | Nullable                       |
| Activo           | bit           |                                |
| FechaCreacion    | datetime2     |                                |

#### `Usuarios` (extiende ASP.NET Identity `IdentityUser`)
| Campo            | Tipo          | Notas                          |
|------------------|---------------|--------------------------------|
| NombreCompleto   | nvarchar(200) |                                |
| Rol              | int (enum)    | Admin=0, Gerente=1, Supervisor=2, Repartidor=3 |
| Activo           | bit           |                                |

#### `UsuarioNegocio` (tabla pivote)
| Campo            | Tipo      | Notas                                        |
|------------------|-----------|----------------------------------------------|
| UsuarioId        | nvarchar (FK Identity) |                               |
| NegocioId        | Guid (FK) |                                              |
| EsNegocioPrincipal | bit     | Para Supervisor: el único negocio asignado   |

#### `ClientesNegocio`
| Campo              | Tipo          | Notas                              |
|--------------------|---------------|------------------------------------|
| Id                 | Guid (PK)     |                                    |
| NegocioId          | Guid (FK)     |                                    |
| NombreCliente      | nvarchar(200) |                                    |
| DireccionEntrega   | nvarchar(500) | Nullable                           |
| HoraAproximada     | time          | Hora de entrega estimada           |
| DiasEntrega        | int           | Bitmask: Lun=1,Mar=2,Mie=4,Jue=8,Vie=16,Sab=32,Dom=64 |
| Activo             | bit           |                                    |

#### `ProductosNegocio`
| Campo            | Tipo          | Notas                                   |
|------------------|---------------|-----------------------------------------|
| Id               | Guid (PK)     |                                         |
| NegocioId        | Guid (FK)     |                                         |
| NombreProducto   | nvarchar(200) |                                         |
| TipoMedida       | int (enum)    | Pieza=0, Gramaje=1                      |
| PrecioPorUnidad  | decimal(18,2) | Por pieza o por KG base                 |
| Activo           | bit           |                                         |

#### `ClienteProductos` (cartera de productos por cliente)
| Campo            | Tipo          | Notas                                   |
|------------------|---------------|-----------------------------------------|
| Id               | Guid (PK)     |                                         |
| ClienteNegocioId | Guid (FK)     |                                         |
| ProductoId       | Guid (FK)     |                                         |
| CantidadHabitual | decimal(18,3) | Cantidad típica que compra              |
| PrecioEspecial   | decimal(18,2) | Nullable — precio acordado con cliente  |

#### `JornadasReparto`
| Campo            | Tipo      | Notas                                        |
|------------------|-----------|----------------------------------------------|
| Id               | Guid (PK) |                                              |
| RepartidorId     | nvarchar (FK Identity) |                               |
| NegocioId        | Guid (FK) |                                              |
| Fecha            | date      |                                              |
| Estado           | int (enum)| Abierta=0, Cerrada=1, Revisada=2             |
| FechaApertura    | datetime2 |                                              |
| FechaCierre      | datetime2 | Nullable                                     |

#### `RondasReparto`
| Campo            | Tipo          | Notas                                    |
|------------------|---------------|------------------------------------------|
| Id               | Guid (PK)     |                                          |
| JornadaId        | Guid (FK)     |                                          |
| NumeroRonda      | int           | 1, 2, 3...                               |
| EtiquetaRonda    | nvarchar(50)  | "Mañana", "Tarde", "Noche", o personalizado |
| Orden            | int           | Para ordenamiento visual                 |

#### `DetallesRonda`
| Campo              | Tipo          | Notas                                    |
|--------------------|---------------|------------------------------------------|
| Id                 | Guid (PK)     |                                          |
| RondaId            | Guid (FK)     |                                          |
| ClienteNegocioId   | Guid (FK)     |                                          |
| ProductoId         | Guid (FK)     |                                          |
| CantidadEntregada  | decimal(18,3) | Piezas o KG                              |
| CantidadDevuelta   | decimal(18,3) |                                          |
| PrecioUnitario     | decimal(18,2) | Snapshot del precio al momento           |
| GramajePreset      | int (enum)    | Ninguno=0, CuartoKg=1, MedioKg=2, UnKg=3, Personalizado=4 |

#### `ClientesJornada` (config por cliente en la jornada)
| Campo                    | Tipo      | Notas                                    |
|--------------------------|-----------|------------------------------------------|
| Id                       | Guid (PK) |                                          |
| JornadaId                | Guid (FK) |                                          |
| ClienteNegocioId         | Guid (FK) |                                          |
| ExcluidoDeEfectivo       | bit       | Si paga con otro método                  |
| MetodoPagoAlternativo    | int (enum)| Efectivo=0, TCredito=1, TDebito=2, Transferencia=3 |
| NotaExclusion            | nvarchar(200) | Nullable                             |

### 1.3 Enums (en `OctagramDelivery.Shared`)

```csharp
public enum RolUsuario { Admin = 0, Gerente = 1, Supervisor = 2, Repartidor = 3 }
public enum TipoMedida { Pieza = 0, Gramaje = 1 }
public enum EstadoJornada { Abierta = 0, Cerrada = 1, Revisada = 2 }
public enum GramajePreset { Ninguno = 0, CuartoKg = 1, MedioKg = 2, UnKg = 3, Personalizado = 4 }
public enum MetodoPago { Efectivo = 0, TarjetaCredito = 1, TarjetaDebito = 2, Transferencia = 3 }
```

### 1.4 Seed inicial

- Rol: `Admin` con usuario `admin@octagram.mx` / contraseña configurable vía `appsettings`.
- Tabla de roles ASP.NET Identity: `Admin`, `Gerente`, `Supervisor`, `Repartidor`.

---

## Fase 2: Backend — ASP.NET Core API

**Entregable:** API funcional con todos los endpoints documentados en Swagger, autenticación JWT operativa, SignalR hub para dashboards en tiempo real.

### 2.1 Estructura del proyecto API

```
OctagramDelivery.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── NegociosController.cs
│   ├── UsuariosController.cs
│   ├── ClientesController.cs
│   ├── ProductosController.cs
│   ├── JornadasController.cs
│   ├── RondasController.cs
│   └── DashboardController.cs
├── Hubs/
│   └── JornadaHub.cs              ← SignalR: actualizaciones en tiempo real
├── Services/
│   ├── AuthService.cs
│   ├── JornadaCalculatorService.cs ← motor de cálculos
│   └── DashboardService.cs
├── Middleware/
│   └── RoleAuthorizationMiddleware.cs
└── Data/
    ├── AppDbContext.cs
    └── Migrations/
```

### 2.2 Autenticación (JWT)

- Login → retorna `AccessToken` (15 min) + `RefreshToken` (7 días).
- Refresh endpoint para renovar silenciosamente.
- Claims incluidos en el token: `UserId`, `Rol`, `NegociosAsignados[]`, `NombreCompleto`.
- Policies:
  - `"AdminOnly"` → Rol == Admin
  - `"GerenteUp"` → Rol <= Gerente
  - `"SupervisorUp"` → Rol <= Supervisor
  - `"RepartidorUp"` → cualquier rol autenticado

### 2.3 Endpoints por módulo

#### Auth
| Método | Ruta                    | Descripción                          |
|--------|-------------------------|--------------------------------------|
| POST   | `/api/auth/login`       | Login con email/password             |
| POST   | `/api/auth/refresh`     | Renovar AccessToken                  |
| POST   | `/api/auth/logout`      | Invalida refresh token               |

#### Negocios (Admin/Gerente)
| Método | Ruta                    | Descripción                          |
|--------|-------------------------|--------------------------------------|
| GET    | `/api/negocios`         | Listar negocios accesibles al usuario|
| POST   | `/api/negocios`         | Crear negocio (Admin)                |
| PUT    | `/api/negocios/{id}`    | Editar negocio                       |
| DELETE | `/api/negocios/{id}`    | Soft delete                          |

#### Usuarios
| Método | Ruta                        | Descripción                         |
|--------|-----------------------------|-------------------------------------|
| GET    | `/api/usuarios`             | Listar (filtrado por rol del caller)|
| POST   | `/api/usuarios`             | Crear usuario                       |
| PUT    | `/api/usuarios/{id}`        | Editar                              |
| POST   | `/api/usuarios/{id}/negocios` | Asignar negocios al usuario       |
| DELETE | `/api/usuarios/{id}`        | Desactivar                          |

#### Clientes y Productos (Gerente/Supervisor)
| Método | Ruta                              | Descripción                      |
|--------|-----------------------------------|----------------------------------|
| GET    | `/api/negocios/{id}/clientes`     | Cartera de clientes              |
| POST   | `/api/negocios/{id}/clientes`     | Agregar cliente                  |
| PUT    | `/api/clientes/{id}`              | Editar cliente                   |
| GET    | `/api/negocios/{id}/productos`    | Catálogo de productos            |
| POST   | `/api/negocios/{id}/productos`    | Agregar producto                 |
| POST   | `/api/clientes/{id}/productos`    | Asignar productos a cliente      |

#### Jornadas (Core del sistema)
| Método | Ruta                                | Descripción                         |
|--------|-------------------------------------|-------------------------------------|
| GET    | `/api/jornadas/hoy`                 | Jornada activa del repartidor       |
| POST   | `/api/jornadas/abrir`               | Abrir nueva jornada del día         |
| POST   | `/api/jornadas/{id}/cerrar`         | Cerrar jornada                      |
| POST   | `/api/jornadas/{id}/rondas`         | Agregar ronda a la jornada          |
| DELETE | `/api/rondas/{id}`                  | Eliminar ronda                      |
| PUT    | `/api/rondas/{id}/detalles`         | **Bulk upsert** de todos los detalles de la ronda (captura rápida) |
| PUT    | `/api/jornadas/{id}/clientes/{clienteId}/exclusion` | Toggle exclusión efectivo |
| GET    | `/api/jornadas/{id}/totales`        | Totales calculados de la jornada    |

#### Dashboard
| Método | Ruta                               | Descripción                          |
|--------|------------------------------------|--------------------------------------|
| GET    | `/api/dashboard/supervisor`        | Todos los repartidores del negocio   |
| GET    | `/api/dashboard/gerente`           | Todos los negocios del gerente       |
| GET    | `/api/dashboard/admin`             | Vista global de todos los negocios   |
| GET    | `/api/dashboard/jornada/{id}`      | Detalle completo de una jornada      |

### 2.4 Motor de cálculos (`JornadaCalculatorService`)

Este servicio centraliza toda la lógica de totales para evitar duplicar cálculos en cliente y servidor:

```csharp
// Resultado de cálculo por jornada
public class TotalesJornada
{
    // Por cliente
    public List<TotalesCliente> PorCliente { get; set; }
    
    // Totales globales
    public decimal TotalEntregadoBruto { get; set; }      // Sin descontar devoluciones
    public decimal TotalDevuelto { get; set; }
    public decimal TotalVentaNeta { get; set; }           // Bruto - Devuelto
    public decimal TotalEfectivo { get; set; }            // Solo clientes NO excluidos
    public decimal TotalExcluidoDeEfectivo { get; set; }  // Para mostrar como aviso
    
    // Por producto (globales)
    public List<TotalesProducto> PorProducto { get; set; }
}

public class TotalesCliente
{
    public Guid ClienteId { get; set; }
    public string NombreCliente { get; set; }
    public bool ExcluidoDeEfectivo { get; set; }
    public MetodoPago? MetodoAlternativo { get; set; }
    
    // Por ronda
    public List<TotalesRonda> PorRonda { get; set; }
    
    // Totales del día (sumatorio de rondas)
    public decimal TotalEntregadoBruto { get; set; }
    public decimal TotalDevuelto { get; set; }
    public decimal TotalVentaNeta { get; set; }
    
    // Por producto en este cliente
    public List<TotalesProductoCliente> PorProducto { get; set; }
}
```

### 2.5 SignalR Hub (`JornadaHub`)

Para que supervisores y gerentes vean actualizaciones en tiempo real sin refrescar:

```csharp
// Grupos: "negocio-{negocioId}", "jornada-{jornadaId}"
// Eventos emitidos:
// - "JornadaActualizada" → cuando un repartidor guarda datos
// - "RondaAgregada"
// - "ClienteExcluidoToggle"
```

---

## Fase 3: PWA del Repartidor (Frontend Core)

**Entregable:** Vista de hoja de cálculo completamente funcional, captura rápida por vuelta, totales en tiempo real, exclusión de clientes, instalable como PWA.

### 3.1 Flujo del repartidor

```
Login
  ↓
Ver Jornada del día (o "Abrir Jornada")
  ↓
Hoja de cálculo: Clientes × Rondas
  ↓
Capturar por celda: cantidad entregada + devolución
  ↓
Panel de totales actualizado automáticamente (cálculo en JS/C# client-side)
  ↓
Guardar automático cada 3s (debounce) o al cambiar celda
  ↓
Cerrar Jornada → resumen final → "Efectivo a entregar: $X,XXX.XX"
```

### 3.2 Vista de Hoja de Cálculo (`/repartidor/jornada`)

**Layout:**

```
┌─ PageHeader: "Jornada del 4 Jun · Negocio La Michoacana" ─────────────────────────────┐
│  [+ Agregar Ronda]  [Cerrar Jornada]                                                   │
├────────────────────────────────────────────────────────────────────────────────────────┤
│                  │        RONDA 1          │        RONDA 2          │   TOTAL DÍA    │
│ CLIENTE          │ Producto A │ Producto B │ Producto A │ Producto B │ Neto │ $Total  │
├──────────────────┼────────────┼────────────┼────────────┼────────────┼──────┼─────────┤
│ [☑] Juan García  │  Ent│Dev   │  Ent│Dev   │  Ent│Dev   │  Ent│Dev   │  12  │ $480    │
│   HH:MM aprox    │   5 │  0   │   3 │  1   │   4 │  0   │   2 │  1   │      │         │
├──────────────────┼────────────┼────────────┼────────────┼────────────┼──────┼─────────┤
│ [☑] Ana López    │  Ent│Dev   │  Ent│Dev   │  Ent│Dev   │  Ent│Dev   │   8  │ $320    │
│ [⚠ Excluido: TC] │            │            │            │            │      │  (excl) │
├──────────────────┼────────────┼────────────┼────────────┼────────────┼──────┼─────────┤
│ TOTALES          │ XX │ XX   │ XX │ XX   │ XX │ XX   │ XX │ XX   │      │         │
│ (por producto)   │ $X,XXX     │ $X,XXX     │ $X,XXX     │ $X,XXX     │      │         │
└──────────────────────────────────────────────────────────────────────────────────────┘
│ GRAN TOTAL: Entregado $X,XXX | Devuelto $X,XXX | Neto $X,XXX | EFECTIVO A RENDIR: $X,XXX │
└────────────────────────────────────────────────────────────────────────────────────────────┘
```

**Implementación técnica:**

- `MudDataGrid` con columnas dinámicas generadas por número de rondas.
- Cada celda es un `MudNumericField` con `Immediate="true"` para captura rápida.
- Para gramaje: al hacer foco en el campo, aparece un `MudPopover` con botones rápidos: `¼KG`, `½KG`, `1KG`, `Personalizado`.
- El checkbox de exclusión muestra un `MudDialog` para confirmar método de pago alternativo.
- Los totales se recalculan en C# client-side (sin llamada API) al modificar cualquier valor — `StateHasChanged()` reactivo.
- Guardado automático: `System.Threading.Timer` a 3 segundos de inactividad → llama `PUT /api/rondas/{id}/detalles` (bulk upsert).
- Indicador visual de estado de guardado: chip pequeño "Guardado ✓" / "Guardando..." / "Error al guardar".

### 3.3 Panel inferior de totales (sticky bottom)

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│  Entregado Bruto    │   Devuelto Total   │   Venta Neta      │   EFECTIVO A RENDIR      │
│  [em] $12,400.00    │  [or] $1,200.00    │  [vi] $11,200.00  │  [in] $9,800.00          │
│                     │                    │                    │  (excluidos: $1,400.00)  │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

Los KpiCards usan los colores semánticos MeridianUI: `--em` para ingresos, `--or` para devoluciones, `--vi` para neto, `--in` para efectivo a rendir.

### 3.4 Gestión de rondas

- Botón `[+ Agregar Ronda]` → `MudDialog` para nombrar la ronda (o nombres automáticos: "Mañana", "Tarde", "Noche").
- Las rondas se renderizan como columnas dinámicas en el grid.
- Botón `[×]` en el header de cada ronda para eliminar (con confirmación vía `MudDialog`).
- Mínimo: 1 ronda. Sin límite máximo.

### 3.5 Pantalla de cierre de jornada

Al pulsar "Cerrar Jornada":
1. `MudDialog` con resumen completo de totales.
2. Firma/confirmación: "El efectivo a entregar es **$X,XXX.XX**. ¿Confirmar cierre?"
3. Acción: POST `/api/jornadas/{id}/cerrar` → estado cambia a `Cerrada`.
4. Redirige a pantalla de "Jornada cerrada — gracias" con opción de ver historial.

### 3.6 Configuración PWA

**`manifest.webmanifest`:**
```json
{
  "name": "OctagramDelivery",
  "short_name": "Octagram",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#F2F3F7",
  "theme_color": "#1976D2",
  "icons": [
    { "src": "icons/icon-192.png", "sizes": "192x192", "type": "image/png" },
    { "src": "icons/icon-512.png", "sizes": "512x512", "type": "image/png" }
  ]
}
```

**Service Worker:** Caché de assets estáticos (CSS, JS, WASM dlls). Para datos dinámicos, estrategia Network-First con fallback a caché para evitar errores offline.

---

## Fase 4: Dashboards de supervisión

**Entregable:** Paneles diferenciados por rol con actualización en tiempo real vía SignalR.

### 4.1 Dashboard Supervisor (`/supervisor/dashboard`)

**Acceso:** Rol `Supervisor` — ve solo su negocio asignado.

```
┌─ PageHeader: "Dashboard · La Michoacana" ──────────────────────────────────────────┐
│  [Fecha selector]  [Estado: 3 activos / 1 cerrado]                                  │
├─────────────────────────────────────────────────────────────────────────────────────┤
│  KpiCards fila:                                                                      │
│  Repartidores Activos [em]  │  Efectivo en Calle [vi]  │  Devoluciones [or]  │  ... │
├─────────────────────────────────────────────────────────────────────────────────────┤
│  Tabla de repartidores:                                                              │
│  Nombre │ Estado │ Clientes │ Entregado │ Devuelto │ Neto │ Efectivo │ Acciones      │
│  Juan   │ Activo │ 12/15    │ $8,400    │ $200     │$8,200│ $7,800   │ [Ver detalle] │
│  Ana    │ Cerrado│ 15/15    │ $12,000   │ $400     │$11,600│$10,200  │ [Ver detalle] │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

- Cada fila expandible muestra el detalle por producto del repartidor.
- "Ver detalle" abre `MudDialog` con la vista read-only de la jornada del repartidor.
- Actualización en tiempo real vía SignalR (`JornadaHub`) — badge "En vivo ●" verde.

### 4.2 Dashboard Gerente (`/gerente/dashboard`)

**Acceso:** Rol `Gerente` — ve todos sus negocios asignados.

```
┌─ PageHeader: "Vista General · 3 Negocios" ────────────────────────────────────────┐
│  [Fecha selector]                                                                   │
├────────────────────────────────────────────────────────────────────────────────────┤
│  KpiCards por negocio:                                                              │
│  ┌─ La Michoacana ─────────┐  ┌─ El Pastorcito ───────────┐                       │
│  │ 5 repartidores           │  │ 3 repartidores             │                       │
│  │ Neto: $45,200 [em]       │  │ Neto: $28,100 [em]         │                       │
│  │ Efectivo: $38,000 [vi]   │  │ Efectivo: $25,000 [vi]     │                       │
│  └──────────────────────────┘  └────────────────────────────┘                       │
├────────────────────────────────────────────────────────────────────────────────────┤
│  Tabla comparativa de repartidores por negocio (expandible por negocio)             │
└────────────────────────────────────────────────────────────────────────────────────┘
```

### 4.3 Dashboard Admin (`/admin/dashboard`)

**Acceso:** Rol `Admin` — vista global de toda la plataforma.

**Módulos:**
1. **Vista global:** KPI cards del sistema (total negocios activos, total repartidores en campo, efectivo total en calle hoy).
2. **Gestión de Negocios:** CRUD completo. Crear negocio, asignar gerente.
3. **Gestión de Usuarios:** Crear/editar/desactivar usuarios de cualquier rol, asignar a negocios.
4. **Historial de Jornadas:** Tabla con filtros por negocio/repartidor/fecha. Exportar a CSV.
5. **Auditoría:** Log de acciones críticas (cambio de rol, cierre de jornada, exclusiones de efectivo).

---

## Fase 5: Integración UI — MudBlazor + MeridianUI

**Entregable:** CSS de overrides completo, App.razor configurado, shell de navegación funcional.

### 5.1 Archivo `meridian-mud-overrides.css`

```css
/* Sobrescribir tokens de MudBlazor con variables MeridianUI */
:root {
    --mud-palette-primary: var(--primary);
    --mud-palette-primary-darken: var(--primary-dark);
    --mud-palette-primary-lighten: var(--primary-light);
    --mud-palette-success: var(--em);
    --mud-palette-warning: var(--am);
    --mud-palette-error: #F44336;
    --mud-palette-info: var(--in);
    --mud-palette-background: var(--page-bg);
    --mud-palette-surface: var(--white);
    --mud-palette-text-primary: var(--gray-dark);
    --mud-palette-text-secondary: var(--gray-muted);
    --mud-default-borderradius: var(--r-md);
}

/* MudDataGrid — estilo tabla MeridianUI */
.mud-table-head .mud-table-cell {
    font-size: 11px;
    font-weight: 600;
    letter-spacing: 0.6px;
    text-transform: uppercase;
    color: var(--gray-muted);
    background: var(--gray-foot);
}

.mud-table-row:hover {
    background: #FAFAFA !important;
}

/* MudCard → Surface MeridianUI */
.mud-card {
    box-shadow: var(--shadow) !important;
    border-radius: var(--r-lg) !important;
}

/* MudNumericField en celdas del grid */
.ronda-cell .mud-input-root {
    font-size: 13px;
    font-weight: 700;
    font-variant-numeric: tabular-nums;
}

/* Chip de exclusión de efectivo */
.exclusion-chip {
    background: rgba(249, 115, 22, 0.15);
    color: #C2410C;
    font-size: 10px;
    font-weight: 600;
    padding: 2px 8px;
    border-radius: var(--r-sm);
}
```

### 5.2 `App.razor` (carga de recursos)

```html
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>OctagramDelivery</title>
    <base href="/" />
    
    <!-- MeridianUI — cargar primero -->
    <link rel="stylesheet" href="css/meridian/colors_and_type.css" />
    
    <!-- MudBlazor -->
    <link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
    <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
    
    <!-- Override tokens MudBlazor → MeridianUI (después de MudBlazor) -->
    <link rel="stylesheet" href="css/meridian/meridian-mud-overrides.css" />
    
    <!-- Material Symbols Rounded (iconografía MeridianUI) -->
    <link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Rounded:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200&display=block" rel="stylesheet"/>
    
    <!-- App CSS -->
    <link rel="stylesheet" href="css/app.css" />
    
    <!-- PWA -->
    <link rel="manifest" href="manifest.webmanifest" />
    <meta name="theme-color" content="#1976D2" />
</head>
<body class="meridian-base">
    <!-- ... -->
    <script src="_content/MudBlazor/MudBlazor.min.js"></script>
</body>
```

### 5.3 Layout por rol

`MainLayout.razor` detecta el rol del usuario autenticado y renderiza el Shell correspondiente:

```razor
@if (rol == RolUsuario.Repartidor)
{
    <RepartidorLayout>@Body</RepartidorLayout>  <!-- Sidebar mínimo, mobile-first -->
}
else
{
    <Shell NavItems="@GetNavItemsForRol(rol)">   <!-- Shell MeridianUI completo -->
        @Body
    </Shell>
}
```

**NavItems por rol:**

| Supervisor                        | Gerente                           | Admin                              |
|-----------------------------------|-----------------------------------|------------------------------------|
| Dashboard (dashboard)             | Dashboard (dashboard)             | Dashboard (dashboard)              |
| Repartidores (delivery_dining)    | Negocios (store)                  | Negocios (store)                   |
| Clientes (people)                 | Repartidores (delivery_dining)    | Usuarios (manage_accounts)         |
| Productos (inventory)             | Reportes (bar_chart)              | Historial (history)                |
| Configuración (settings)          | Configuración (settings)          | Auditoría (security)               |
|                                   |                                   | Configuración (settings)           |

---

## Fase 6: Despliegue en Ubuntu Server con Docker

**Entregable:** Los tres contenedores corriendo, subdominios activos con SSL, accesibles desde internet.

### 6.1 Estructura en el servidor

```
/root/docker/
│
├── docker-compose.yml          ← Nginx Proxy Manager (ya existente)
├── cloudflared/
│   ├── config.yml              ← Agregar rutas octagram-app y octagram-api
│   └── ...
│
└── apps/
    └── octagram/
        ├── docker-compose.yml  ← Stack completo OctagramDelivery
        ├── api/
        │   └── Dockerfile
        └── client/
            └── Dockerfile
```

### 6.2 Dockerfile — API (`OctagramDelivery.API`)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish OctagramDelivery.API/OctagramDelivery.API.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OctagramDelivery.API.dll"]
```

### 6.3 Dockerfile — Client (Blazor WASM PWA)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish OctagramDelivery.Client/OctagramDelivery.Client.csproj -c Release -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
```

**`nginx.conf` para Blazor WASM (manejo de rutas SPA):**
```nginx
server {
    listen 80;
    root /usr/share/nginx/html;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    # Cache agresivo para assets compilados (WASM, JS)
    location ~* \.(wasm|js|css|png|svg|ico)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
```

### 6.4 `docker-compose.yml` — Stack OctagramDelivery

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

  octagram-api:
    build:
      context: ./api
      dockerfile: Dockerfile
    container_name: octagram-api
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "Server=octagram-db;Database=OctagramDelivery;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=True"
      Jwt__SecretKey: "${JWT_SECRET}"
      Jwt__Issuer: "octagram-api.endevour.mx"
      Jwt__Audience: "octagram-app.endevour.mx"
    expose:
      - "8080"
    depends_on:
      - octagram-db
    networks:
      - octagram-internal
      - proxy

  octagram-app:
    build:
      context: ./client
      dockerfile: Dockerfile
    container_name: octagram-app
    restart: unless-stopped
    expose:
      - "80"
    networks:
      - proxy

volumes:
  octagram-db-data:

networks:
  octagram-internal:
    driver: bridge
  proxy:
    external: true
```

### 6.5 Variables de entorno (`.env` en `/root/docker/apps/octagram/`)

```env
DB_PASSWORD=TuPasswordSeguro123!
JWT_SECRET=TuSecretoJWTMuyLargoYSeguro256bits
```

### 6.6 Actualizar `config.yml` de Cloudflare Tunnel

```yaml
tunnel: TU_TUNNEL_ID
credentials-file: /etc/cloudflared/TU_TUNNEL_ID.json

ingress:
  - hostname: endevour.mx
    service: http://npm:80

  - hostname: octagram-app.endevour.mx
    service: http://npm:80

  - hostname: octagram-api.endevour.mx
    service: http://npm:80

  # ... otros subdominios existentes ...

  - service: http_status:404
```

### 6.7 Nginx Proxy Manager — Proxy Hosts a crear

| Domain                       | Forward Hostname | Port | SSL       |
|------------------------------|------------------|------|-----------|
| `octagram-app.endevour.mx`   | `octagram-app`   | 80   | Force SSL |
| `octagram-api.endevour.mx`   | `octagram-api`   | 8080 | Force SSL |

### 6.8 DNS en Cloudflare

Crear dos registros CNAME:
- `octagram-app` → `TU_TUNNEL_ID.cfargotunnel.com` (Proxy ON)
- `octagram-api` → `TU_TUNNEL_ID.cfargotunnel.com` (Proxy ON)

### 6.9 Comandos de despliegue

```bash
# Primera vez
cd /root/docker/apps/octagram
docker compose up -d --build

# Actualizaciones posteriores
git pull
docker compose up -d --build octagram-api  # Solo la API
# o
docker compose up -d --build                # Todo el stack

# Logs
docker logs octagram-api -f
docker logs octagram-app -f
docker logs octagram-db -f

# Migraciones (ejecutar una sola vez o tras cada migración)
docker exec octagram-api dotnet OctagramDelivery.API.dll migrate
# (o usar un migration runner en el startup de la API)
```

---

## Resumen de fases y entregables

| Fase | Nombre                          | Entregable principal                                              | Prioridad |
|------|---------------------------------|-------------------------------------------------------------------|-----------|
| 0    | Preparación y estructura        | Solución .sln lista, proyectos creados, NuGet instalados          | Alta      |
| 1    | Modelado de base de datos       | Entidades EF Core, migraciones, seed de roles                     | Alta      |
| 2    | Backend API                     | Todos los endpoints operativos, JWT, SignalR hub                  | Alta      |
| 3    | PWA Repartidor                  | Hoja de cálculo funcional, totales en tiempo real, PWA instalable | Alta      |
| 4    | Dashboards supervisión          | Paneles Supervisor, Gerente y Admin con SignalR                   | Media     |
| 5    | Integración MudBlazor+MeridianUI| CSS overrides, Shell, NavMenu por rol                             | Media     |
| 6    | Despliegue Docker               | 3 contenedores corriendo, subdominios activos con SSL             | Alta      |

---

## Convenciones de desarrollo

- **Nomenclatura:** PascalCase en C#, kebab-case en rutas CSS.
- **API responses:** Siempre con formato `{ data, error, message }` para consistencia.
- **Cálculos de dinero:** Siempre `decimal`, nunca `double` o `float`.
- **Gramajes:** Almacenar siempre en KG decimales (`0.25`, `0.5`, `1.0`, o el valor personalizado).
- **Fechas:** `DateTime` en servidor en UTC, convertir a hora local (`America/Mexico_City`) en el cliente.
- **Bulk upsert de ronda:** El endpoint acepta el array completo de detalles; el servidor hace MERGE (Insert o Update) para evitar condiciones de carrera.
- **Soft deletes:** Ninguna entidad se elimina físicamente. Todos tienen campo `Activo = false`.

---

*OctagramDelivery — Plan v2.0 · KeorSoft · Junio 2026*
