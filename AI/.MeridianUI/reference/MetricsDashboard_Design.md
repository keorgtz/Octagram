# Design Spec — ResumenSistemaView
**Sistema:** SHEndevour Hotel System
**Vista:** Resumen del Día (Resumen Operativo)
**Stack:** WPF · MaterialDesignInXaml · MeridianUI · C# / MVVM · Segoe UI

---

## 1. Propósito y Audiencia

Panel de inicio para el **recepcionista de turno**. Da un vistazo completo del estado operativo del hotel en tiempo real: llegadas, salidas, ocupación actual, forecast del día y corte de caja. No requiere interacción compleja — es lectura rápida + botón de refresh.

---

## 2. Layout General

```
┌──────────────────────────────────────────────────────────────────┐
│  HEADER  (título + fecha + botón refresh)           Row 0 – auto │
├────────────┬────────────┬────────────┬───────────────────────────┤
│  KPI       │  KPI       │  KPI       │  KPI              Row 1   │
│  Llegadas  │  Salidas   │  Ocupación │  Forecast         auto    │
├────────────┬────────────┬────────────┬───────────────────────────┤
│  Tabla     │  Tabla     │  Tabla     │  Tabla            Row 2   │
│  Registros │  Rentas    │  Cargos    │  Abonos           *       │
├──────────────────────────────────────────────────────────────────┤
│  FOOTER CAJA  (Efectivo · Tarjetas · Gastos · Total)   Row 3 auto│
└──────────────────────────────────────────────────────────────────┘
```

- **Grid principal:** 4 filas (`auto / auto / * / auto`), sin columnas a nivel raíz.
- **KPI Row:** `Grid` de 4 columnas `*` dentro de Row 1.
- **Tablas Row:** `Grid` de 4 columnas `*` dentro de Row 2.
- **Footer Caja:** `materialDesign:Card` con grid interno de 5 columnas (`* * * 1px *`), donde la col 3 es un separador visual de 1 px.

---

## 3. Background y Superficie

| Elemento | Color |
|---|---|
| Background de la UserControl | `#F2F3F7` (gris muy suave, casi blanco) |
| Cards KPI | `White` |
| Cards de Tablas | `White` |
| Card Footer Caja | `White` |
| Header de tabla (colored strip) | Color semántico por columna (ver §6) |
| Footer de cada tabla | `#F9FAFB` |

---

## 4. Tipografía

| Elemento | Fuente | Tamaño | Weight | Color |
|---|---|---|---|---|
| Título principal "Resumen del Día" | Segoe UI | 22px | SemiBold | `#1C1E26` |
| Subtítulo fecha | Segoe UI | 12px | Normal | `#8B5CF6` (violeta accent) |
| Label KPI (e.g. "LLEGADAS") | Segoe UI | 13px | SemiBold | `#6B7280` |
| Número grande KPI | Segoe UI | 42px | Bold | Color accent de la card |
| Badge % ocupación | Segoe UI | 13px | Bold | Color accent de la card |
| Leyenda dot-label | Segoe UI | 12px | Normal | `#6B7280` |
| Header de tabla – título | Segoe UI | 11px | SemiBold | `#6B7280` |
| Header de tabla – subtítulo | Segoe UI | 12px | Normal | Color accent de la card |
| Columnas DataGrid | MaterialDesign default | — | — | — |
| Total footer de tabla | Segoe UI | 13px | Bold | Color accent de la card |
| Caja – label | Segoe UI | 10px | SemiBold | `#6B7280` |
| Caja – monto | Segoe UI | 18px | Bold | Color accent de la sección |
| Caja – Total | Segoe UI | 22px | Bold | `#8B5CF6` |

---

## 5. Colores Semánticos por Sección

Cada una de las 4 KPI cards y sus tablas correspondientes comparte una familia de color:

| Sección | Color Principal | Variante Media | Variante Suave | Background Pill/Badge | Background Header Card |
|---|---|---|---|---|---|
| **Llegadas** | `#10B981` (Emerald) | `#34D399` | `#6EE7B7` | `#DCFCE7` | `#F0FDF4` |
| **Salidas** | `#F59E0B` (Amber) | `#FBB95A` | `#FDE68A` | `#FEF3C7` | `#FFFBEB` |
| **Ocupación** | `#6366F1` (Indigo) | `#A5B4FC` | — | `#E0E7FF` | `#EEF2FF` |
| **Forecast** | `#8B5CF6` (Violet) | `#C4B5FD` | — | `#EDE9FE` | `#F5F3FF` |
| **Bloqueadas** (en barra) | `#EC4899` (Pink) | — | — | — | — |
| **Libres** (en barra) | `#E5E7EB` (Gray light) | — | — | — | — |

---

## 6. KPI Cards — Anatomía Detallada

Cada card es un `materialDesign:Card` con `UniformCornerRadius="20"` y elevación `Dp1`.

### 6.1 Accent Bar (tope de la card)
```
Border Height="4" VerticalAlignment="Top" CornerRadius="20,20,0,0"
Background = color principal de la sección
```
Crea una franja de color de 4px en la parte superior que identifica visualmente la sección antes de leer el texto.

### 6.2 Estructura interna (Grid 5 filas, Margin="18,14")

```
Row 0 │ DockPanel: Label KPI (izq) + Ícono MeridianSymbol (der)
Row 1 │ TextBlock número grande + Border badge % (solo Ocupación y Forecast)
Row 2 │ Barra segmentada (height 10px, Margin 0,10)
Row 3 │ WrapPanel de leyenda (dots + labels)
```

### 6.3 Barra Segmentada
- `Grid` de 4 columnas con ancho proporcional definido por `StringFormat={}{0}*` enlazado a los `Pct*` del ViewModel.
- Cada `Border` tiene un color del degradado de la sección.
- Las esquinas redondeadas solo en la primera columna (izquierda) y última (derecha).
- Espacio de 1px entre segmentos (`Margin="0,0,1,0"` en los interiores).
- Si el total es 0, todos los segmentos quedan en gris `#E5E7EB`.

### 6.4 Leyenda
- `WrapPanel` con `StackPanel Orientation="Horizontal"` por ítem.
- Cada ítem: `Ellipse` 9×9 (color relleno) + `TextBlock` 12px con el valor enlazado.
- `StringFormat` directo en el binding: `StringFormat=Directos {0}`.

### 6.5 Badge % (Ocupación y Forecast únicamente)
```
Border Background="color suave" CornerRadius="10" Padding="8,4"
  Margin="10,0,0,8" VerticalAlignment="Bottom"
    TextBlock FontSize="13" FontWeight="Bold" Color="color principal"
```
Se alinea a la base del número grande con `VerticalAlignment="Bottom"`.

---

## 7. Íconos por Sección (MeridianUI)

| Sección | Kind | Color |
|---|---|---|
| Llegadas | `flight_land` | `#10B981` |
| Salidas | `flight_takeoff` | `#F59E0B` |
| Ocupación | `Bed` | `#6366F1` |
| Forecast | `pending_actions` | `#8B5CF6` |
| Registros turno | `pending_actions` | `#10B981` |
| Rentas/Extras | `assignment_add` | `#F59E0B` |
| Cargos | `Ballot` | `#6366F1` |
| Abonos | `attach_money` | `#8B5CF6` |
| Caja – Efectivo | `attach_money` | `#10B981` |
| Caja – Tarjetas | `credit_card` | `#6366F1` |
| Caja – Gastos | `shopping_bag` | `#F97316` |
| Caja – Total | `account_balance_wallet` | `#8B5CF6` |
| Refresh button | `refresh` (MeridianSymbol) | `#6B7280` |

Tamaño base: `Size="20"` en KPI cards. `Size="15"` en headers de tablas. `Size="18"` en footer de caja.

---

## 8. Cards de Tablas — Anatomía Detallada

`materialDesign:Card` con `UniformCornerRadius="16"`, elevación `Dp1`, `Background="White"`.

### 8.1 Header de la tabla (Row 0)
```
Border CornerRadius="16,16,0,0" Padding="14,10"
Background = color suave de la sección
  DockPanel:
    StackPanel (izq): Label ALL CAPS 11px + subtítulo descriptivo 12px color principal
    Border (der): badge con Count de la colección, color principal
```

### 8.2 DataGrid (Row 1)
- Style `TurnoGrid` basado en `MaterialDesignDataGrid`.
- `materialDesign:ThemeAssist.Theme="Light"` y `Elevation="Dp0"`.
- `BorderThickness="0"`, `Background="Transparent"`.
- `GridLinesVisibility="Horizontal"`.
- `VirtualizingPanel.IsVirtualizing="True"` con modo `Recycling`.
- Columnas:
  - Registros: `Hab. (40px) | Huésped (*) | Tarifa (80px)`
  - Rentas/Cargos/Abonos: `Cuenta (*) | Movs. (50px) | Total (90px)`
- Las columnas de montos tienen `TextAlignment="Right"` y `FontWeight="SemiBold"`.

### 8.3 Footer de totales (Row 2)
```
Border Background="#F9FAFB" CornerRadius="0,0,16,16" Padding="14,8"
  DockPanel:
    TextBlock (izq): descripción 11px gris
    TextBlock (der): total en color principal 13px Bold
```
El total se calcula vía un `IValueConverter` que suma la colección (`SumRateConverter` para registros, `SumAcumuladoConverter` para el resto). **Nota:** implementar estos converters en la capa de Utilities del proyecto.

---

## 9. Footer de Caja — Anatomía Detallada

`materialDesign:Card` `Background="White"` `UniformCornerRadius="18"` `Elevation="Dp1"`.

Grid interno de 5 cols: `* * * 1px *` con `Margin="10,8"`.

Cada sección (cols 0-2 y 4):
```
StackPanel Orientation="Horizontal" HCenter VCenter Margin="10,8"
  Border (ícono pill): CornerRadius="12" Padding="10" Background=color suave
    MeridianSymbol Size="20" Foreground=color principal
  StackPanel (texto):
    TextBlock label: 10px SemiBold #6B7280 ALL CAPS
    TextBlock monto: 18px Bold color principal (22px para Total)
```

Col 3: `Border Background="#E5E7EB" Width="1" Margin="0,8"` — separador vertical.

---

## 10. Header de la Vista

`DockPanel Margin="16,14,16,4"`:

- **Izquierda:** `StackPanel` con título 22px SemiBold `#1C1E26` y subtítulo fecha 12px `#8B5CF6`.
- **Derecha:** `StackPanel Orientation="Horizontal"`:
  - `ProgressBar` circular `IsIndeterminate="True"` violeta, `Width/Height=22`, visible solo cuando `IsLoading=True` (Converter `BooleanToVisibilityConverter`).
  - `Button` outlined `BorderBrush="#D1D5DB"` `Foreground="#6B7280"` con ícono refresh + texto "Actualizar", `Command=RefreshCommand`.

---

## 11. Spacing y Márgenes

| Contexto | Valor |
|---|---|
| Margin externo de cada KPI Card | `8` (todos lados) |
| Padding interno de KPI Card | `18,14` |
| Margin KPI Row contenedor | `8,0` |
| Margin Tablas Row contenedor | `8,4` |
| Margin Footer Caja contenedor | `8,4,8,10` |
| Header DockPanel | `16,14,16,4` |
| Margin entre leyenda dots | `0,2,10,2` |
| Margin entre barra segmentada | `0,10,0,10` |

---

## 12. Animaciones / Feedback Visual

| Evento | Comportamiento |
|---|---|
| `IsLoading = true` | `ProgressBar` circular violeta aparece junto al botón Refresh |
| `IsLoading = false` | `ProgressBar` colapsa (Visibility Collapsed) |
| Cambio de colección | Las listas se actualizan via `ObservableCollection` — sin animación explícita |
| Hover DataGrid row | MaterialDesign default hover (highlight suave) |

No se usan storyboards ni animaciones de entrada para mantener la vista liviana y apta para actualización frecuente.

---

## 13. ViewModel — Propiedades Clave

### KPI Properties (para binding de barras)

Todos los `Pct*` son `double` entre 0 y 100 que se usan como `{Binding PctX, StringFormat={}{0}*}` en `ColumnDefinition.Width`.

```
PctDirectas / PctReserva / PctProbables          → Llegadas
PctSalidasRealizadas / PctInesperadas / PctProgramadas → Salidas
PctOcupadas / PctUsoCasa / PctBloqueadas / PctLibres   → Ocupación
PctProbOcupadas / PctProbUsoCasa / PctProbBloqueadas / PctProbLibres → Forecast
```

### Contadores (int)
```
TotalLlegadas / LlegadasDirectas / LlegadasReserva / LlegadasProbables
TotalSalidas / SalidasRealizadas / SalidasInesperadas / SalidasProgramadas
TotalHabitaciones / Ocupadas / UsoCasa / Bloqueadas / Libres
TotalProbable / ProbOcupadas / ProbUsoCasa / ProbBloqueadas / ProbLibres
```

### Badges de porcentaje (double)
```
PorcentajeOcupacion   → mostrado en badge de la card Ocupación
PorcentajeProbable    → mostrado en badge de la card Forecast
```

### Colecciones de tablas
```
ObservableCollection<RegistroTurnoDTO> RegistrosTurno
ObservableCollection<MovimientoTurnoDTO> RentasTurno
ObservableCollection<MovimientoTurnoDTO> CargosTurno
ObservableCollection<MovimientoTurnoDTO> AbonosTurno
```

### Caja
```
decimal Efectivo
decimal TarjetasYOtros
decimal GastosYRetiros
decimal TotalCaja
```

### Estado
```
bool IsLoading          → controla ProgressBar
string FechaDisplay     → "jueves, 21 de mayo de 2026" (CultureInfo es-MX)
IRelayCommand RefreshCommand
```

---

## 14. Converters Necesarios

Estos converters deben existir en `SHEndevour.Utilities` y registrarse en los recursos globales de la App:

| Converter | Key en ResourceDictionary | Descripción |
|---|---|---|
| `BooleanToVisibilityConverter` | `BooleanToVisibilityConverter` | Estándar MaterialDesign, ya incluido |
| `SumRateConverter` | `SumRateConverter` | Suma `Rate` de una `IEnumerable<RegistroTurnoDTO>` |
| `SumAcumuladoConverter` | `SumAcumuladoConverter` | Suma `Acumulated` de una `IEnumerable<MovimientoTurnoDTO>` |

Alternativa más simple: exponer propiedades computadas en el ViewModel (e.g. `TotalRegistros`, `TotalRentas`) y bindear directo, evitando los converters de colección.

---

## 15. Notas de Implementación

1. **DataContext:** La vista instancia su propio `ResumenSistemaViewModel` en XAML (`<vm:ResumenSistemaViewModel />`). Si se prefiere inyección desde el shell, eliminar esa línea y asignar desde el NavigationViewModel.

2. **ColumnDefinition Width con Pct*:** WPF acepta `Width="{Binding PctX, StringFormat={}{0}*}"` en `ColumnDefinition`. Si el valor llega en 0 para todas, la columna del "resto" (la cuarta, siempre `1*`) actúa como fallback visual mostrando la barra completa en gris.

3. **IsRoomCharge / IsExpense:** Estos campos en `MovimientoTabla` determinan la separación de Rentas vs Cargos vs Gastos de caja. Si no existen como columnas, ajustar la query en `LoadTablasAsync` y `LoadCajaAsync` según el esquema real.

4. **Caja — claves de pago:** La separación Efectivo vs Tarjetas se hace por `PaymentKey == "EFECTIVO"`. Ajustar el literal a la clave real del catálogo de formas de pago del sistema.

5. **Refresh automático:** Considerar un `DispatcherTimer` de 5-10 minutos en el constructor del ViewModel para recargar sin intervención del usuario durante un turno largo.

6. **Performance:** Todas las queries corren en `Task.Run()` para no bloquear el UI thread. El update de colecciones y propiedades se hace via `Dispatcher.InvokeAsync()`.

---

## 16. Paleta de Colores Resumida

```
Emerald  #10B981  #34D399  #6EE7B7  #DCFCE7  #F0FDF4
Amber    #F59E0B  #FBB95A  #FDE68A  #FEF3C7  #FFFBEB
Indigo   #6366F1  #A5B4FC  —        #E0E7FF  #EEF2FF
Violet   #8B5CF6  #C4B5FD  —        #EDE9FE  #F5F3FF
Pink     #EC4899  —        —        —        —
Gray     #E5E7EB  #F9FAFB  #6B7280  #1C1E26  —
Orange   #F97316  #FFF7ED  —        —        —
```
