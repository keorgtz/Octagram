# ReprocesoDashboard — Design Specification
**SHEndevour · Módulo Gerencial**
Versión: 1.0 · Plataforma: WPF .NET 9 · UI Library: MaterialDesignInXaml

---

## 1. Propósito y Contexto

El **ReprocesoDashboard** es una herramienta interna de uso gerencial diario.
Su objetivo es clasificar las notas de venta de un rango de fechas para separar
las notas **fiscales** (venta al público con nota de venta / CFDI) de las
**no fiscales** (efectivo / ticket), renumerarlas correctamente y reflejar
el resultado en el sistema contable/fiscal del hotel.

**Usuarios:** Gerentes de turno, Administradores de hotel.
**Frecuencia de uso:** Diaria, al cierre de cada turno o jornada.
**Criticidad:** Alta — afecta numeración fiscal y reportes SAT.

---

## 2. Paleta de Colores

### Colores base (heredados de MaterialDesignInXaml)
| Token | Uso |
|---|---|
| `PrimaryHueDarkBrush` | Header principal de la vista |
| `PrimaryHueMidBrush` | Acentos, KPI primary, botón Buscar |
| `MaterialDesignBackground` | Fondo general del UserControl |
| `MaterialDesignCardBackground` | Fondo de Cards y filas de movimientos |
| `MaterialDesignToolBarBackground` | Footer y fondo de detalle expandido |
| `MaterialDesignBody` | Texto principal |
| `MaterialDesignBodyLight` | Etiquetas secundarias, hints |
| `MaterialDesignDivider` | Separadores y bordes |

### Colores semánticos (definidos como StaticResource)
| Key | Hex | Uso |
|---|---|---|
| `CargoColor` | `#2196F3` | Badge y monto de movimientos tipo Cargo |
| `AbonoColor` | `#4CAF50` | Badge y monto de movimientos tipo Abono |
| `DesctoColor` | `#FF9800` | Badge y monto de movimientos tipo Descuento |
| `DevuelColor` | `#F44336` | Badge y monto de movimientos tipo Devolución |

### Colores de estado de nota
| Estado | Fondo chip | Color texto/ícono |
|---|---|---|
| Facturada (CFDI) | `#1A4CAF50` | `#4CAF50` |
| Sin factura | `#1AFF9800` | `#FF9800` |

### Colores de forma de pago (chip FormaPago)
| Clave SAT | Forma de pago | Ícono MD | Color |
|---|---|---|---|
| `01` | Efectivo | `Cash` | `#4CAF50` |
| `03` | Transferencia | `BankTransfer` | `#FF9800` |
| `04` | Tarjeta crédito | `CreditCard` | `#2196F3` |
| `28` | Tarjeta débito | `CreditCard` | `#9C27B0` |
| `99` / otros | Por definir | `HelpCircleOutline` | `#9E9E9E` |

### Colores de mini-KPIs del footer
| KPI | Fondo | Borde | Texto |
|---|---|---|---|
| A Reprocesar (seleccionadas) | `#E8F5E9` | `#A5D6A7` | `#2E7D32` |
| Ventas al Público (no sel.) | `#FFF3E0` | `#FFCC80` | `#BF360C` |
| Ya Facturadas | `#E3F2FD` | `#90CAF9` | `#0D47A1` |

---

## 3. Tipografía

| Elemento | FontFamily | FontSize | FontWeight |
|---|---|---|---|
| Título "REPROCESO" | Montserrat | 20 | Bold |
| Subtítulo header | (heredada) | 12 | Regular |
| Etiquetas de columna (NOTA, CLIENTE…) | (heredada) | 9 | Bold |
| Número de nota | (heredada) | 14 | Bold |
| Cliente / Origen | (heredada) | 13 / 11 | Regular |
| Total Neto en card | (heredada) | 15 | Bold |
| Forma de pago chip | (heredada) | 10–11 | SemiBold |
| KPI número grande | (heredada) | 28 | Bold |
| KPI total general | (heredada) | 28 | Bold |
| Mini-KPI footer | (heredada) | 16 | Bold |
| Label mini-KPI | (heredada) | 8 | Bold |
| Movimiento concepto | (heredada) | 12 | Regular |
| Movimiento grupo | (heredada) | 10 | Regular |
| Movimiento monto | (heredada) | 13 | SemiBold |

---

## 4. Estructura de Layout

```
┌─────────────────────────────────────────────────────────┐
│  ROW 0 — HEADER (PrimaryHueDark)                        │
│  [🔄 REPROCESO]  [Clasificación de notas]   [● spinner] │
├─────────────────────────────────────────────────────────┤
│  ROW 1 — BARRA DE BÚSQUEDA (Card Dp2)                   │
│  [📅] [FechaInicio] — [FechaFin] | estado | [Todas][Ninguna] [Buscar]│
├─────────────────────────────────────────────────────────┤
│  ROW 2 — BARRA DE ORDENAMIENTO (Card Dp1)               │
│  Ordenar por: [# Nota ↕] [Cliente ↕] [F.Pago ↕] [Total ↕] [Facturadas ↕] [Origen ↕]│
├─────────────────────────────────────────────────────────┤
│  ROW 3 — KPI CARDS (5 columnas iguales)                 │
│  [Total] [Seleccionadas] [Facturadas] [Sin Factura] [Total General]│
├─────────────────────────────────────────────────────────┤
│  ROW 4 — LISTA DE NOTAS (ScrollViewer *)                │
│  ┌──────────────────────────────────────────────────┐   │
│  │ [☐] NOTA  ORIGEN  CLIENTE  TOTAL  [FormaPago]  ↕│   │
│  ├──────────────────────────────────────────────────┤   │
│  │   [DETALLE expandido — movimientos]              │   │
│  └──────────────────────────────────────────────────┘   │
│  (repite por cada nota)                                  │
├─────────────────────────────────────────────────────────┤
│  ROW 5 — FOOTER                                          │
│  [A Reprocesar $] [Ventas Público $] [Facturadas $]  [Resultado] [REPROCESAR]│
└─────────────────────────────────────────────────────────┘
```

### Grid.RowDefinitions
```xml
Auto   <!-- Header -->
Auto   <!-- Búsqueda -->
Auto   <!-- Ordenamiento -->
Auto   <!-- KPIs -->
*      <!-- Lista (toma espacio restante) -->
Auto   <!-- Footer -->
```

---

## 5. Componentes

### 5.1 Header
- `Border` con `Background=PrimaryHueDarkBrush`, `Padding="20,14"`
- Ícono `Refresh` 28×28 blanco + título + subtítulo
- `ProgressBar` circular `IsIndeterminate` alineado a la derecha,
  visible solo cuando `IsBuscando = true`

### 5.2 Barra de Búsqueda (Card Dp2, CornerRadius 10)
Columnas: Ícono | DatePicker inicio | TextBlock "—" | DatePicker fin | Separator | MensajeEstado | [Todas][Ninguna] | [Buscar]

- **DatePickers**: `MaterialDesignOutlinedDatePicker`, width 160 c/u
- **Botones Todas/Ninguna**: `MaterialDesignOutlinedButton`, height 36
- **Botón Buscar**: `MaterialDesignRaisedButton`, height 40, `IsEnabled = !IsBuscando` vía `InverseBoolConverter`
- **MensajeEstado**: texto italic, `TextTrimming=CharacterEllipsis`

### 5.3 Barra de Ordenamiento (Card Dp1, CornerRadius 8)
- `StackPanel` horizontal con ícono `SortVariant` + label "Ordenar por:"
- 6 botones `MaterialDesignFlatButton` con `CommandParameter` = nombre de propiedad
- Cada botón muestra ícono `UnfoldMoreHorizontal` / `SortAscending` / `SortDescending`
  según `OrdenActual` y `OrdenAscendente` via `MultiDataTrigger`
- El botón activo muestra el ícono en `PrimaryHueMidBrush`

**Campos de orden disponibles:**
| Label | CommandParameter | Propiedad en SaleNoteWrapper |
|---|---|---|
| # Nota | `NumSaleNote` | `int?` |
| Cliente | `ClienteDisplay` | `string` |
| Forma de pago | `FormaPagoMayoritaria` | `string` |
| Total | `TotalNeta` | `decimal` |
| Facturadas | `IsFacturada` | `bool` |
| Origen | `OrigenDisplay` | `string` |

### 5.4 KPI Cards (5 columnas, Card Dp1, CornerRadius 8)
Cada card tiene: ícono + label 9px Bold + número 28px Bold

| # | Label | Binding | Color número |
|---|---|---|---|
| 1 | TOTAL NOTAS | `TotalNotas` | `PrimaryHueMidBrush` |
| 2 | SELECCIONADAS | `NotasSeleccionadas` | `#9C27B0` |
| 3 | FACTURADAS | `NotasFacturadas` | `#4CAF50` |
| 4 | SIN FACTURA | `NotasNoFacturadas` | `#FF9800` |
| 5 | TOTAL GENERAL | `TotalGeneral` (C2) | `#323232` |

### 5.5 Lista de Notas — NotaCardTemplate (Card Dp1, CornerRadius 8)

#### Header de cada card (siempre visible)
```
[CheckBox] [NOTA / número] [ORIGEN / texto] [CLIENTE / nombre] [TOTAL NETO / $] [FormaPago + Facturada] [N movs ↕]
```

| Columna | Width | Contenido |
|---|---|---|
| Checkbox | Auto | `IsEnabled = !IsFacturada` via InverseBool |
| Número nota | 70 | Label "NOTA" 9px + valor 14px Bold |
| Origen | 140 | Label "ORIGEN" 9px + texto 11px |
| Cliente | * | Label "CLIENTE" 9px + texto 13px |
| Total Neto | 120 | Label "TOTAL NETO" 9px + valor 15px Bold PrimaryHue |
| Chip(s) | 120 | Chip FormaPago + chip "Facturada" (solo si IsFacturada) |
| Botón expand | Auto | Contador movimientos + chevron |

**Chip FormaPago:** `Border CornerRadius=12`, `Padding=10,3`
Fondo `#1A2196F3` (varía por clave), ícono + texto coloreados por clave SAT

**Chip Facturada:** Solo visible si `IsFacturada=true`
`Border CornerRadius=8`, fondo `#1A4CAF50`, ícono `CheckCircle` verde

**Botón expand:** `MaterialDesignIconButton`, muestra count de movimientos
y ícono `ChevronDown` / `ChevronUp` según `DetalleExpandido`

#### Detalle expandible (colapsado por defecto)
`Border` con `Background=#e9e9e9`, visible cuando `DetalleExpandido=true`

Contiene:
- Fila de encabezado: label "MOVIMIENTOS" + mini-chips "Cargos: $X" y "Abonos: $X"
- `ItemsControl` con `MovimientoTemplate`

#### MovimientoTemplate
```
[Badge tipo] [AccountDescription / MovementHasGroup] [Reference] [AccountTotalAmount $]
```

| Columna | Width | Contenido |
|---|---|---|
| Badge | 90 | `Border` coloreado por `AccountType` + texto `MovementTypeDisplay` |
| Concepto | * | `AccountDescription` 12px + `MovementHasGroup` 10px light |
| Referencia | 120 | `Reference` 11px, light, CharacterEllipsis |
| Monto | 110 | `AccountTotalAmount` C2, 13px SemiBold, coloreado por tipo |

### 5.6 Estado vacío (placeholder)
Visible cuando `Notas.Count == 0` en el ScrollViewer.
Ícono `FileSearch` 56×56 + texto "No hay notas cargadas" + instrucción.

### 5.7 Footer (Border con ToolBarBackground)
Layout: `Grid` con 2 columnas: `*` (KPIs) y `Auto` (resultado + botón)

**Mini-KPIs (StackPanel horizontal, izquierda):**

| KPI | Binding | Color |
|---|---|---|
| A REPROCESAR | `TotalEfectivo` | Verde `#2E7D32` |
| VENTAS AL PÚBLICO | `TotalNoSeleccionadas` | Naranja `#BF360C` |
| YA FACTURADAS | `TotalFacturadas` | Azul `#0D47A1` |

Cada uno: `Border CornerRadius=8 Padding=12,8` + ícono + label 8px + valor 16px Bold

**Botón REPROCESAR (derecha):**
- `MaterialDesignRaisedButton`, CornerRadius 8, height 44
- Ícono `Refresh` + texto "REPROCESAR NOTAS" + subtexto "{N} seleccionada(s)"
- `IsEnabled = !IsBuscando` via InverseBoolConverter

**Resultado reproceso** (aparece después de ejecutar):
- `Border` verde `#E8F5E9`, visible cuando `MostrarResultadoReproceso=true`
- Ícono `CheckCircleOutline` + texto del resultado

---

## 6. Converters requeridos

| Converter | Namespace | Uso |
|---|---|---|
| `BooleanToVisibilityConverter` | WPF nativo | Spinner, detalle expandido, resultado, chip facturada |
| `InverseBoolConverter` | `SHEndevour.Helpers.Converters` | IsEnabled en botones Buscar y Reprocesar, IsEnabled en Checkbox |

---

## 7. Bindings principales (ViewModel → View)

| Propiedad VM | Tipo | Usado en |
|---|---|---|
| `FechaInicio` | `DateTime` | DatePicker inicio |
| `FechaFin` | `DateTime` | DatePicker fin |
| `IsBuscando` | `bool` | Spinner, IsEnabled botones |
| `MensajeEstado` | `string` | TextBlock estado |
| `NotasView` | `ICollectionView` | ItemsControl lista |
| `OrdenActual` | `string` | MultiDataTrigger en botones orden |
| `OrdenAscendente` | `bool` | MultiDataTrigger en botones orden |
| `TotalNotas` | `int` | KPI 1 |
| `NotasSeleccionadas` | `int` | KPI 2 |
| `NotasFacturadas` | `int` | KPI 3 |
| `NotasNoFacturadas` | `int` | KPI 4 |
| `TotalGeneral` | `decimal` | KPI 5 |
| `TotalEfectivo` | `decimal` | Mini-KPI footer 1 |
| `TotalNoSeleccionadas` | `decimal` | Mini-KPI footer 2 |
| `TotalFacturadas` | `decimal` | Mini-KPI footer 3 |
| `MostrarResultadoReproceso` | `bool` | Visibility resultado |
| `ResultadoReproceso` | `string` | Texto resultado |

### Bindings del SaleNoteWrapper (dentro de DataTemplate)
| Propiedad | Tipo | Usado en |
|---|---|---|
| `Seleccionada` | `bool` | CheckBox.IsChecked (TwoWay) |
| `DetalleExpandido` | `bool` | Visibility detalle, ícono chevron |
| `NumSaleNote` | `int?` | Número de nota |
| `OrigenDisplay` | `string` | Origen |
| `ClienteDisplay` | `string` | Cliente |
| `TotalNeta` | `decimal` | Total neto |
| `TotalCargos` | `decimal` | Mini-resumen detalle |
| `TotalAbonos` | `decimal` | Mini-resumen detalle |
| `IsFacturada` | `bool` | Chip facturada, IsEnabled checkbox |
| `FormaPagoMayoritaria` | `string` | Texto chip forma de pago |
| `FormaPagoClave` | `string` | DataTrigger ícono/color chip |
| `CantMovimientos` | `int` | Contador botón expand |
| `Movimientos` | `IEnumerable<MovementModel>` | ItemsControl movimientos |

---

## 8. Comandos

| Comando (VM) | Tipo | Parámetro | Efecto |
|---|---|---|---|
| `BuscarNotasCommand` | `AsyncRelayCommand` | — | Carga notas del rango de fechas |
| `OrdenarCommand` | `RelayCommand` | `string` campo | Ordena `NotasView` por la propiedad indicada |
| `SeleccionarTodasCommand` | `RelayCommand` | — | Selecciona todas las no facturadas |
| `DeseleccionarTodasCommand` | `RelayCommand` | — | Deselecciona todas las no facturadas |
| `NotaSeleccionadaChangedCommand` | `RelayCommand` | — | Recalcula KPIs del footer |
| `ToggleDetalleCommand` | `RelayCommand` | `SaleNoteWrapper` | Expande/colapsa el detalle de movimientos |
| `ReprocesarNotasCommand` | `AsyncRelayCommand` | — | Ejecuta `ReprocessService`, recarga lista |
| `GenerarFacturaGlobalCommand` | `RelayCommand` | — | Abre `InformacionGlobalDialog` |

---

## 9. Lógica de negocio relevante

### Auto-selección al buscar
```
Seleccionada = !EsEfectivo && !IsNS && !IsFacturada
```
Las notas seleccionadas son las **fiscales** (para venta al público).
Las no seleccionadas son las de efectivo/ticket.

### TotalNeta por nota
```
TotalNeta = Σ AccountTotalAmount donde AccountType ∈ {Cargo, Descuento}
```
Los descuentos ya vienen negativos en BD, se suman directamente.

### FormaPagoMayoritaria
Solo considera movimientos `AccountType = Abono` con `Payment != null`.
Agrupa por `Payment.ClaveProductoSat`, toma el grupo con mayor suma.

### EsEfectivo
```
EsEfectivo = FormaPagoClave == "01"
```

### TotalesFooter
```
TotalEfectivo      = Σ TotalNeta de notas (Seleccionada && !IsFacturada)
TotalNoSeleccionadas = Σ TotalNeta de notas (!Seleccionada && !IsFacturada)
TotalFacturadas    = Σ TotalNeta de notas (IsFacturada)
```

---

## 10. Archivos del módulo

| Archivo | Tipo | Descripción |
|---|---|---|
| `Views/Gerencial/ReprocesoView.xaml` | View | UI principal |
| `Views/Gerencial/ReprocesoView.xaml.cs` | Code-behind | Solo inicialización, sin lógica |
| `ViewModels/Gerencial/ReprocesoViewModel.cs` | ViewModel | Incluye `SaleNoteWrapper` |
| `Services/Gerencial/ReprocessService.cs` | Service | Lógica de renumeración y marcado IsNS |
| `Helpers/Converters/InverseBoolConverter.cs` | Converter | Invierte bool para IsEnabled |
