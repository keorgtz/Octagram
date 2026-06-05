# MeridianUI — Design System
> Versión 1.0 · Sistema de diseño oficial SHEndevour Web

---

## 1. Filosofía de diseño

**Tono:** Enterprise-soft — funcional, denso en información, pero visualmente cómodo. Sin brutalismo ni experimentalismo; cada decisión estética sirve a la legibilidad y la eficiencia operativa.

**Principios:**
- **Densidad controlada** — mucha información en pantalla, nunca abrumadora.
- **Color semántico** — cada color carga significado: verde = ok/ingreso, ámbar = alerta/pendiente, índigo = neutro/referencia, violeta = resumen/total, naranja = gasto/salida, rojo = error/cancelación.
- **Elevación sutil** — sombras ligeras definen jerarquía sin oscurecer.
- **Interacción silenciosa** — hover states, transiciones cortas (≤ 200 ms), sin animaciones ruidosas.
- **Consistencia absoluta** — el mismo token CSS para el mismo concepto, en cualquier módulo.

---

## 2. Paleta de colores

### 2.1 Colores semánticos (sistema de 5 niveles)

Cada color semántico tiene 5 variantes: `strong` → `mid` → `light` → `pale` → `bg`.

| Rol            | Token base  | Strong     | Mid        | Light      | Pale       | Bg         |
|----------------|-------------|------------|------------|------------|------------|------------|
| **Emerald** – OK / Ingreso / Éxito | `--em`  | `#10B981` | `#34D399` | `#6EE7B7` | `#DCFCE7` | `#F0FDF4` |
| **Amber** – Alerta / Pendiente / Rentas | `--am` | `#F59E0B` | `#FBB95A` | `#FDE68A` | `#FEF3C7` | `#FFFBEB` |
| **Indigo** – Neutro / Referencia / Cargos | `--in` | `#6366F1` | `#A5B4FC` | `#C7D2FE` | `#E0E7FF` | `#EEF2FF` |
| **Violet** – Total / Resumen / Abonos | `--vi` | `#8B5CF6` | `#C4B5FD` | `#DDD6FE` | `#EDE9FE` | `#F5F3FF` |
| **Orange** – Gasto / Salida / Advertencia | `--or` | `#F97316` | `#FB923C` | `#FED7AA` | `#FFEDD5` | `#FFF7ED` |

### 2.2 Colores de acción Material (herencia de módulos WPF)

Usados exclusivamente en módulos con lógica CFDI / facturación / reproceso:

| Rol                   | Valor      |
|-----------------------|------------|
| Cargo / débito        | `#2196F3`  |
| Abono / crédito       | `#4CAF50`  |
| Descuento             | `#FF9800`  |
| Devolución / error    | `#F44336`  |
| Facturado             | `#4CAF50`  |
| Efectivo              | `#4CAF50`  |
| Tarjeta crédito       | `#2196F3`  |
| Tarjeta débito        | `#9C27B0`  |
| Transferencia         | `#FF9800`  |
| Sin definir           | `#9E9E9E`  |

### 2.3 Neutros

| Token            | Valor     | Uso principal                        |
|------------------|-----------|--------------------------------------|
| `--gray-dark`    | `#1C1E26` | Texto principal                      |
| `--gray-700`     | `#374151` | Texto secundario / celdas de tabla   |
| `--gray-muted`   | `#6B7280` | Labels, metadatos                    |
| `--gray-line`    | `#E5E7EB` | Bordes, divisores                    |
| `--gray-foot`    | `#F9FAFB` | Fondos de footer de tarjeta          |
| `--page-bg`      | `#F2F3F7` | Fondo de página (área de contenido)  |
| `--sidebar-bg`   | `#1C1E26` | Fondo sidebar                        |
| `--sidebar-active`| `#2A2D3A`| Ítem activo en sidebar               |
| `--white`        | `#FFFFFF` | Superficies de tarjetas              |

### 2.4 Primario de acción (botones CTA)

| Token            | Valor     |
|------------------|-----------|
| `--primary`      | `#1976D2` |
| `--primary-dark` | `#1565C0` |
| `--primary-light`| `#BBDEFB` |

---

## 3. Tipografía

### Stack

```
font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
/* Display / Headers en módulos WPF-origin: */
font-family: 'Montserrat', 'Segoe UI', sans-serif;
```

### Escala tipográfica

| Rol                     | Tamaño  | Peso   | Color           | Notas                              |
|-------------------------|---------|--------|-----------------|------------------------------------|
| Page title              | 22 px   | 600    | `--gray-dark`   |                                    |
| Section title           | 16–18 px| 700    | `--gray-dark`   | Montserrat en módulos WPF-origin   |
| Card title / KPI label  | 11 px   | 600    | `--gray-muted`  | UPPERCASE, letter-spacing 0.6 px   |
| KPI value               | 28–42 px| 700    | color semántico |                                    |
| Body / tabla            | 12–13 px| 400    | `#374151`       |                                    |
| Meta / sub-label        | 9–11 px | 600    | `--gray-muted`  | UPPERCASE, letter-spacing 0.8 px   |
| Monospace (importes)    | 13–15 px| 700    | semántico       | `font-variant-numeric: tabular-nums`|

---

## 4. Espaciado y radios

### Sistema de espaciado (base 4 px)

| Token   | Valor | Uso típico                          |
|---------|-------|-------------------------------------|
| `--s1`  | 4 px  | Gap mínimo, padding badge           |
| `--s2`  | 8 px  | Gap entre chips, padding mini       |
| `--s3`  | 12 px | Padding interno de tarjeta pequeña  |
| `--s4`  | 16 px | Padding estándar de tarjeta         |
| `--s5`  | 20 px | Padding de sección / header         |
| `--s6`  | 24 px | Margen entre secciones              |

### Radios

| Token    | Valor | Uso                                       |
|----------|-------|-------------------------------------------|
| `--r-sm` | 4–6 px| Badges, chips pequeños, inputs           |
| `--r-md` | 8–10 px| Botones, inputs normales, chips pago    |
| `--r-lg` | 12–16 px| Tarjetas normales                      |
| `--r-xl` | 18–20 px| Tarjetas KPI, caja footer              |

---

## 5. Elevación (sombras)

| Nivel | CSS value                                              | Uso                              |
|-------|--------------------------------------------------------|----------------------------------|
| Dp0   | ninguna                                                | Elementos en bg de página        |
| Dp1   | `0 1px 4px rgba(0,0,0,.08), 0 0 0 .5px rgba(0,0,0,.06)`| Tarjetas por defecto (shadow var)|
| Dp2   | `0 2px 6px rgba(0,0,0,.15), 0 1px 3px rgba(0,0,0,.12)`| Barra de búsqueda, dropdowns    |
| Dp3   | `0 4px 16px rgba(0,0,0,.16)`                           | Dialogs, modales, tooltips       |

---

## 6. Layout de la aplicación

```
┌─────────────────────────────────────────────────┐
│  TITLEBAR  (altura: 48 px, fija, bg: sidebar)   │
├──────────┬──────────────────────────────────────┤
│          │  CONTENT HEADER  (54 px, sticky)     │
│ SIDEBAR  ├──────────────────────────────────────┤
│ (220 px) │                                      │
│  fijo    │  MAIN CONTENT  (flex:1, scroll)      │
│          │                                      │
├──────────┴──────────────────────────────────────┤
│  STATUS BAR  (altura: 28 px, fija, bg: sidebar) │
└─────────────────────────────────────────────────┘
```

### 6.1 Titlebar
- **Alto:** 48 px · **Fondo:** `#FFFFFF` · **Borde inferior:** 1 px `--gray-line` + shadow Dp1
- Contenido: logo + nombre app (izquierda) · breadcrumb (centro) · chips / avatar / íconos (derecha)
- Texto principal: `--gray-dark` · Texto secundario: `--gray-muted`

### 6.2 Sidebar
- **Ancho:** 220 px expandido · 64 px colapsado (solo íconos)
- **Fondo:** `#FFFFFF` (blanco, mismo que titlebar y statusbar)
- **Borde derecho:** 1 px `--gray-line`
- Ítem activo: fondo `--in-bg` (#EEF2FF), border-radius `--r-md` (sin borde lateral), texto e ícono en `--in` (#6366F1), sombra `0 1px 3px rgba(99,102,241,.12)`
- Ítem hover: fondo `#F0F4FF`, texto e ícono en `--in`
- Ítem normal: padding `8px 12px`, margin `1px 8px`, borde-radius `--r-md`
- Ícono: 18 px, color `--gray-muted` → `--in` (hover/active)
- Label: 13 px Segoe UI 500, `--gray-muted` → `--in` 600 (active)
- Separadores de grupo: línea `--gray-line` + label 9 px uppercase `--gray-muted`

### 6.3 Content Header (por módulo)
- **Alto:** 54 px · **Fondo:** `--white` · **Borde inferior:** 1 px `--gray-line`
- Contiene: breadcrumb/título de módulo (izquierda) + acciones contextuales (derecha)

### 6.4 Main Content
- **Fondo:** `--page-bg` (#F2F3F7)
- Padding: 12–16 px por los lados
- Scroll interno (el layout externo nunca scrollea)

### 6.5 Status Bar
- **Alto:** 28 px · **Fondo:** `#FFFFFF` · **Borde superior:** 1 px `--gray-line`
- Contenido: estado de conexión / turno activo (izquierda) · versión / fecha-hora (derecha)
- Fuente: 11 px, Segoe UI 400, color `--gray-muted`
- Separadores entre ítems: 1 px `--gray-line`

---

## 7. Componentes

### 7.1 KPI Card
```
┌──────────────────────────┐
│ ▬▬▬▬ (accent bar 4px)   │ ← color semántico, border-radius top
│  LABEL        [icon]     │ ← 11px uppercase muted
│  42           +12%       │ ← 42px bold semántico + badge
│  ▓▓▓▓▓░░░░░░ seg-bar    │ ← opcional
│  ● A  ● B  ● C  legend   │ ← opcional
└──────────────────────────┘
radius: --r-xl | shadow: Dp1 | padding: 12px 18px 16px
```

### 7.2 Table Card
```
┌─ colored header (bg semántico) ──────────────┐
│ LABEL UPPERCASE     [count badge]            │
│ Sub-label colored                            │
├──────────────────────────────────────────────┤
│ tabla (sticky thead, hover row)              │
├──────────────────────────────────────────────┤
│ footer gray: label    Total $XX,XXX          │
└──────────────────────────────────────────────┘
radius: --r-lg | shadow: Dp1
```

### 7.3 Nota / Row Card (módulos operativos)
```
┌─ checkbox │ num │ origen │ cliente │ total │ chips │ expand ─┐
│           (grid de 7 columnas, altura ~54px)                 │
├─ detalle expandible (bg #e9e9e9) ────────────────────────────┤
│ MOVIMIENTOS header          Cargos $X  Abonos $X             │
│ [badge tipo] concepto / grupo   ref       monto              │
└──────────────────────────────────────────────────────────────┘
```

### 7.4 Chip / Badge de pago
| Forma pago   | Fondo                    | Color texto | Ícono   |
|-------------|--------------------------|-------------|---------|
| Efectivo    | `rgba(76,175,80,.15)`    | `#388E3C`   | payments|
| T. Crédito  | `rgba(33,150,243,.15)`   | `#1565C0`   | credit_card|
| T. Débito   | `rgba(156,39,176,.15)`   | `#7B1FA2`   | credit_card|
| Transferencia| `rgba(255,152,0,.15)`   | `#E65100`   | swap_horiz|
| Sin definir | `rgba(158,158,158,.15)`  | `#616161`   | help_outline|
| Facturada   | `rgba(76,175,80,.15)`    | `#388E3C`   | check_circle|

### 7.5 Botones

| Tipo         | Estilo                                                              |
|--------------|---------------------------------------------------------------------|
| Primary/CTA  | bg `--primary`, color #fff, radius `--r-md`, shadow Dp1, h 36–44 px|
| Outlined     | border 1.5px `--primary`, color `--primary`, bg transparent        |
| Flat/Text    | bg transparent, color `--primary`, hover bg rgba(25,118,210,.08)   |
| Danger       | bg `#F44336`, color #fff (solo reproceso / acciones destructivas)   |
| Disabled     | bg `#BDBDBD`, color #fff, cursor not-allowed                        |

### 7.6 Inputs / DatePicker
- Border: 1.5 px `--primary` en focus, 1.5 px `--gray-line` en rest
- Border-radius: `--r-md` (8 px)
- Fondo: `#FFFFFF`
- Label flotante: 10 px, color `--primary`, bg white

### 7.7 Footer mini-KPIs (módulos operativos)
Tres bloques con borde coloreado:
- **Verde** (A reprocesar / ingreso): bg `#E8F5E9`, border `#A5D6A7`
- **Naranja** (Ventas al público / gasto): bg `#FFF3E0`, border `#FFCC80`
- **Azul** (Ya facturadas / referencia): bg `#E3F2FD`, border `#90CAF9`

### 7.8 Barra segmentada
- Altura: 10 px · border-radius: 6 px · gap: 1 px entre segmentos
- Cada segmento usa color semántico `strong` o `mid`

---

## 8. Iconografía

Librería: **Material Icons Round** (Google Fonts CDN).

| Concepto          | Ícono                  |
|-------------------|------------------------|
| Dashboard/Home    | `dashboard`            |
| Reproceso         | `refresh`              |
| Métricas/Turno    | `bar_chart`            |
| Huéspedes         | `people`               |
| Reservas          | `event`                |
| Habitaciones      | `bed`                  |
| Facturación       | `receipt_long`         |
| Restaurante       | `restaurant`           |
| Caja              | `account_balance_wallet`|
| Configuración     | `settings`             |
| Buscar            | `search`               |
| Calendario        | `calendar_today`       |
| Ordenar           | `sort`                 |
| Expandir/Colapsar | `expand_more`          |
| Check/Ok          | `check_circle`         |
| Alerta            | `warning`              |
| Cerrar sesión     | `logout`               |

---

## 9. Animaciones y transiciones

| Evento              | Propiedad            | Duración  | Easing          |
|---------------------|----------------------|-----------|-----------------|
| Hover botón/card    | background, box-shadow| 150 ms   | ease            |
| Expand nota-detail  | opacity + translateY | 180 ms    | ease            |
| Sort icon           | content (swap)       | inmediato |                 |
| Spinner             | rotate               | 800 ms    | linear infinite |
| Sidebar collapse    | width                | 200 ms    | ease-in-out     |
| Slide-in notificación| translateX          | 250 ms    | ease-out        |

---

## 10. Grids de contenido

| Contexto                  | Grid                              |
|---------------------------|-----------------------------------|
| KPI row (4 cards)         | `repeat(4, 1fr)`, gap 12px        |
| KPI row (5 cards)         | `repeat(5, 1fr)`, gap 8px         |
| Tables row (4 columnas)   | `repeat(4, 1fr)`, gap 12px        |
| Tables row (2 columnas)   | `repeat(2, 1fr)`, gap 12px        |
| Footer caja               | flex row, `flex: 1` por ítem      |
| Footer mini-KPIs          | flex row, gap 8px                 |

---

## 11. Sidebar — estructura de navegación

```
┌──────────────────────┐
│  [logo] SHEndevour   │  ← 48px titlebar integrado
├──────────────────────┤
│  ▌ Dashboard    🏠   │  ← activo
│    Métricas     📊   │
├── OPERACIONES ───────┤  ← label grupo
│    Registros    🛏   │
│    Reservas     📅   │
│    Caja         💰   │
├── GERENCIAL ─────────┤
│    Reproceso    🔄   │
│    Facturación  🧾   │
│    Reportes     📋   │
├── SISTEMA ───────────┤
│    Configuración ⚙  │
│    Usuarios      👤  │
└──────────────────────┘
│  [avatar] Turno M    │  ← footer sidebar
└──────────────────────┘
```

---

## 12. Status Bar — contenido

**Izquierda:**
`● Conectado` · `Turno: Matutino (08:00 – 16:00)` · `Usuario: J. Rodríguez`

**Derecha:**
`Hotel Misión GDL` · `v2.4.1` · `Jue 21 May 2026 · 14:32`

Separadores: `·` en `rgba(255,255,255,.35)`

---

## 13. Estados de datos en tablas y listas

| Estado              | Visual                                              |
|---------------------|-----------------------------------------------------|
| Vacío               | ícono 56px + texto 16px + subtexto 12px, centrado   |
| Cargando            | Spinner 28px circular, `dodgerblue` sobre bg oscuro |
| Error               | ícono `error_outline` rojo + mensaje + btn reintentar|
| Hover fila          | bg `#FAFAFA`                                        |
| Fila seleccionada   | border-left 3px `--primary` + bg `#EEF2FF`          |
| Deshabilitado       | opacity 0.5, cursor not-allowed                     |

---

## 14. Tokens CSS — variables completas

```css
:root {
  /* ── Semánticos ── */
  --em: #10B981; --em-md: #34D399; --em-lt: #6EE7B7; --em-pl: #DCFCE7; --em-bg: #F0FDF4;
  --am: #F59E0B; --am-md: #FBB95A; --am-lt: #FDE68A; --am-pl: #FEF3C7; --am-bg: #FFFBEB;
  --in: #6366F1; --in-md: #A5B4FC; --in-lt: #C7D2FE; --in-pl: #E0E7FF; --in-bg: #EEF2FF;
  --vi: #8B5CF6; --vi-md: #C4B5FD; --vi-lt: #DDD6FE; --vi-pl: #EDE9FE; --vi-bg: #F5F3FF;
  --or: #F97316; --or-md: #FB923C; --or-pl: #FFEDD5; --or-bg: #FFF7ED;
  --pk: #EC4899;

  /* ── Material (módulos CFDI) ── */
  --cargo:  #2196F3;
  --abono:  #4CAF50;
  --dscto:  #FF9800;
  --devol:  #F44336;

  /* ── Primario de acción ── */
  --primary:      #1976D2;
  --primary-dark: #1565C0;
  --primary-light:#BBDEFB;

  /* ── Neutros ── */
  --gray-dark:  #1C1E26;
  --gray-700:   #374151;
  --gray-muted: #6B7280;
  --gray-line:  #E5E7EB;
  --gray-foot:  #F9FAFB;
  --page-bg:    #F2F3F7;
  --white:      #FFFFFF;

  /* ── App shell — Light theme ── */
  --sidebar-bg:     #FFFFFF;
  --sidebar-active: #F0FDF4;   /* em-bg — verde muy suave */
  --sidebar-border: #E5E7EB;   /* gray-line */
  --titlebar-h:     48px;
  --statusbar-h:    28px;
  --sidebar-w:      220px;
  --sidebar-w-col:  64px;

  /* ── Tipografía ── */
  --font:  'Segoe UI', system-ui, sans-serif;
  --font-display: 'Montserrat', 'Segoe UI', sans-serif;

  /* ── Radios ── */
  --r-sm: 6px; --r-md: 10px; --r-lg: 14px; --r-xl: 20px;

  /* ── Sombras ── */
  --shadow:   0 1px 4px rgba(0,0,0,.08), 0 0 0 .5px rgba(0,0,0,.06);
  --shadow-2: 0 2px 6px rgba(0,0,0,.15), 0 1px 3px rgba(0,0,0,.12);
  --shadow-3: 0 4px 16px rgba(0,0,0,.16);
}
```

---

## 15. No hacer (anti-patterns)

- ❌ Usar gradientes como fondos de página o tarjetas
- ❌ Sombras dramáticas o `drop-shadow` en texto
- ❌ Más de 5 colores en una sola vista
- ❌ Font-size < 9 px
- ❌ Animaciones > 300 ms en interacciones de lista
- ❌ Bordes de tarjeta con color (solo shadow Dp1)
- ❌ Íconos sin label en sidebar expandido
- ❌ Tablas sin sticky thead cuando el scroll es posible
- ❌ Botones sin estado disabled cuando hay acción async activa
- ❌ Mezclar colores semánticos entre módulos (verde de éxito no es verde de efectivo en otro contexto)
