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
