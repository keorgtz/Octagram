# Keorsoft · MeridianUI Design System

> El sistema de diseño visual de **Keorsoft** y el lenguaje **MeridianUI** — usado en SHEndevour y demás productos de software de escritorio y web de la empresa.

---

## 1 · ¿Qué es Keorsoft?

**Keorsoft** es una empresa dedicada a crear **soluciones de software de escritorio y web** con un foco en:

- Alta personalización por cliente / vertical
- Excelente UI / UX, densa pero confortable
- Un lenguaje visual propio llamado **MeridianUI**

### MeridianUI

MeridianUI es un dialecto que toma referencias de:

- **Fluent Design** (Microsoft) — superficies claras, jerarquía suave por elevación, accent de color preciso.
- **Material Expressive** (Google) — color semántico vivo, iconografía Round, ritmo tipográfico.
- **MaterialDesignInXaml** — la base técnica en módulos WPF/.NET.
- **El toque Keorsoft** — densidad controlada para operaciones de back-office, color semántico en 5 niveles, sin gradientes ruidosos, animación silenciosa (≤ 200 ms).

> Tono general: **Enterprise-soft.** Funcional, denso, visualmente cómodo. Sin brutalismo ni experimentos.

---

## 2 · Productos representados

Este sistema se deriva del producto interno principal de Keorsoft:

### **SHEndevour** — Hotel Management System

Plataforma de gestión hotelera. Existe en **dos plataformas paralelas** que comparten MeridianUI:

| Plataforma | Stack | Audiencia |
|------------|-------|-----------|
| **SHEndevour Desktop** | WPF · .NET 9 · MaterialDesignInXaml · Segoe UI / Montserrat | Recepcionistas y gerentes que operan el hotel en sitio |
| **SHEndevour Web** | HTML/CSS · Material Icons Round · Segoe UI / Montserrat | Mismo público, acceso remoto y móvil |

Módulos representados en este sistema (cada uno con su spec):

1. **MetricsDashboard / Resumen del Día** — vista de inicio del recepcionista (KPIs de llegadas, salidas, ocupación, forecast + tablas de movimientos + caja).
2. **ReprocesoDashboard** — herramienta gerencial diaria para clasificar notas fiscales / no fiscales.
3. **AccountStateDashboard** — estado de cuenta / detalle de movimientos.

---

## 3 · Fuentes (sources de este sistema)

| Source | Path en el proyecto | Tipo |
|--------|---------------------|------|
| `Claude/SHEDashboard_Design.md` (codebase mounted) | `reference/SHEDashboard_Design.md` | Spec maestro MeridianUI Web |
| `Claude/MetricsDashboard_Design.md` | `reference/MetricsDashboard_Design.md` | Spec módulo Resumen del Día (WPF) |
| `Claude/AccountStateDashboard_Design.md` | `reference/AccountStateDashboard_Design.md` | Spec módulo Reproceso (WPF) |
| `Claude/SHEDashboard.html` | `reference/SHEDashboard.html` | Implementación HTML del shell + dashboard |
| `Claude/MetricsDashboard_Preview.html` | `reference/MetricsDashboard_Preview.html` | Preview HTML del Resumen del Día |
| `Claude/ReprocesoDashboard.html` | `reference/ReprocesoDashboard.html` | Preview HTML del Reproceso |
| Subida del 22-May | `reference/MeridianUI_Design.md` | **Spec consolidado v1.0** — fuente más reciente de MeridianUI |
| Subida del 22-May | `reference/MeridianUI_Preview.html` | Preview oficial del dashboard MeridianUI v1.0 |

> No hay repositorio GitHub público ni archivo Figma asociado. Si el lector tiene acceso al codebase original (carpeta `Claude/` que el usuario montó), el contenido está bajo `Claude/*`.

---

## 4 · Index del sistema

```
.
├── README.md                  ← este archivo
├── SKILL.md                   ← skill para Claude Code / agentes
├── colors_and_type.css        ← tokens CSS (color + tipografía + spacing)
├── fonts/                     ← (Segoe UI no se redistribuye; ver §Type)
├── assets/                    ← logos, icon-sheet, imagen-marca
├── preview/                   ← cards del Design System tab
│   ├── colors-semantic.html
│   ├── colors-neutrals.html
│   ├── colors-payments.html
│   ├── type-scale.html
│   ├── type-display.html
│   ├── radii.html
│   ├── shadows.html
│   ├── spacing.html
│   ├── buttons.html
│   ├── chips.html
│   ├── inputs.html
│   ├── kpi-card.html
│   ├── table-card.html
│   ├── nota-card.html
│   ├── sidebar.html
│   ├── titlebar.html
│   ├── icons.html
│   └── logo.html
├── ui_kits/
│   └── shendevour-web/        ← UI kit del producto web SHEndevour
│       ├── README.md
│       ├── index.html         ← prototipo clickeable
│       ├── tokens.css         ← copia local de los tokens
│       └── components/        ← JSX components
└── reference/                 ← specs originales (read-only)
```

---

## 5 · Content Fundamentals

> ¿Cómo se escribe la copy en Keorsoft / SHEndevour?

### Idioma y tono

- **Español neutro de México.** Toda la UI está en español. Términos técnicos fiscales se mantienen en español (CFDI, SAT, ticket, abono, cargo).
- **Tono enterprise-soft, segunda persona implícita.** Se le habla al usuario pero sin usar “tú”/“usted” explícitos en labels — los verbos van en infinitivo o imperativo neutro: *“Buscar”*, *“Reprocesar notas”*, *“Seleccionar todas”*, *“Actualizar”*.
- **No hay marketing voice.** Es software operativo. La copy es funcional, no entusiasta. No hay “¡Bienvenido!”, no hay “🎉”, no hay emoji.
- **Densidad sobre amabilidad.** La UI no acompaña al usuario con frases largas; muestra datos. Mensajes vacíos son la única excepción: *“No hay notas cargadas”* + instrucción corta para corregir.

### Casing

- **Títulos de módulo:** UPPERCASE con letter-spacing (`REPROCESO`, `RESUMEN DEL DÍA`). Para nombres de productos largos se usa Title Case (`Resumen del Día`, `Métricas del Turno`).
- **Labels de KPI y meta:** UPPERCASE 9–11 px, letter-spacing 0.06–0.12em (`TOTAL NOTAS`, `LLEGADAS`, `A REPROCESAR`).
- **Botones:** Sentence case (`Buscar`, `Actualizar`, `Cerrar sesión`). Excepción: el botón de acción destructiva o destacada va en UPPERCASE (`REPROCESAR NOTAS`).
- **Texto de tabla y body:** Sentence case normal.
- **Categorías de sidebar:** UPPERCASE 9 px (`OPERACIONES`, `GERENCIAL`, `SISTEMA`).

### Ejemplos reales (extraídos de la fuente)

| Contexto | Copy |
|----------|------|
| Página vacía | *“No hay notas cargadas”* + *“Selecciona un rango de fechas y presiona Buscar.”* |
| Estado de conexión | *“● Conectado · Turno: Matutino (08:00 – 16:00) · Usuario: J. Rodríguez”* |
| Botón destacado | *“REPROCESAR NOTAS”* (sub-texto: *“{N} seleccionada(s)”*) |
| Chip de estado | *“Facturada”*, *“Sin factura”*, *“Efectivo”* |
| Mini-KPI footer | *“A REPROCESAR”* · *“VENTAS AL PÚBLICO”* · *“YA FACTURADAS”* |
| Subtítulo de tabla | *“jueves, 21 de mayo de 2026”* (formato `dddd, d 'de' MMMM 'de' yyyy`, CultureInfo `es-MX`) |
| Breadcrumb | *“Hotel Misión GDL › Dashboard”* |

### Emoji / decoración

- **Emoji: ❌ no.** No se usan en UI. La iconografía es Material Icons Round.
- **Caracteres unicode decorativos:** sólo `●` (bullet) para estados de conexión y `›` (single right angle) para breadcrumbs.
- **No exclamaciones, no signos de pregunta de cierre falsos, no ALL CAPS para énfasis** (sólo para labels meta).

---

---

## 6 · Visual Foundations

### 6.1 Filosofía visual

**Enterprise-soft.** Cada decisión estética sirve a la legibilidad y la eficiencia operativa. Sin brutalismo, sin experimentalismo, sin gradientes ruidosos. Cada color carga significado.

Principios MeridianUI:

- **Densidad controlada** — mucha información en pantalla sin abrumar.
- **Color semántico** — verde = ok/ingreso, ámbar = alerta/pendiente, índigo = neutro/referencia, violeta = total/resumen, naranja = gasto, rojo = error.
- **Elevación sutil** — sombras Dp1 definen jerarquía sin oscurecer la página.
- **Interacción silenciosa** — transiciones cortas (≤ 200 ms), hover suaves, sin bounces.
- **Consistencia absoluta** — el mismo token CSS para el mismo concepto en cualquier módulo.

### 6.2 Color

Sistema de **5 niveles** por familia semántica (`strong / mid / light / pale / bg`). Ver `colors_and_type.css` y `preview/colors-semantic.html`.

**Reglas de uso:**

- Nunca > 5 colores en una sola vista.
- No mezclar verdes — el verde Emerald (`#10B981`) es éxito/llegada en módulos web; el verde Material (`#4CAF50`) es abono/CFDI. Nunca se cruzan en el mismo contexto.
- Bordes de tarjeta: **shadow Dp1**, no colored border. Excepción: mini-KPI con borde coloreado (`#A5D6A7`, `#FFCC80`, `#90CAF9`) sobre fondo pale.

### 6.3 Tipografía

| Stack | Uso |
|-------|-----|
| `'Coco Gothic'` | **EXCLUSIVO para wordmarks de marca** — “Keorsoft”, “SHEndevour” y los nombres de los demás softwares de la casa. **No aparece en UI corriente** y se entrega como asset estático (PNG/SVG) porque es licenciada (Type Depot). |
| `'Inter'` (con fallback `'Segoe UI'`) | Todo el producto: body, UI, labels, tablas, KPI grandes. |

> **Regla:** si estás escribiendo HTML / CSS para una pantalla del producto, **no uses `--font-brand`**. Sólo aplica `font-family: var(--font-brand)` cuando estás renderizando el wordmark exacto de Keorsoft o de uno de sus softwares. En cualquier otro contexto usa `var(--font)`.

> **Disclaimer técnico:** Coco Gothic es comercial. En este sistema, el `--font-brand` cae a Inter como fallback web. Donde aparezca un wordmark en HTML real, debe servirse como `<img>` PNG/SVG renderizado con Coco Gothic auténtico (export desde el archivo maestro), nunca como texto vivo.

- Escala: 9 / 10 / 11 / 12 / 13 / 14 / 16 / 18 / 22 / 28 / 42 px.
- Pesos: 400 / 500 / 600 / 700 / 800.
- **Labels** siempre UPPERCASE con `letter-spacing: 0.06–0.12em`.
- **Importes** siempre con `font-variant-numeric: tabular-nums`.
- **Mínimo legible:** 9 px (sólo metas en labels). El body nunca baja de 12 px.

> ⚠️ **Sobre Segoe UI:** la documentación original del producto WPF estaba escrita asumiendo Segoe UI. En la versión web actual del design system, Inter es la fuente primaria — coincide con la métrica de Segoe UI sin licencia y carga gratis desde Google Fonts. Si el cliente exige Segoe UI exacta en web, hay que sumar los `.ttf` con licencia bajo `fonts/`.

### 6.4 Espaciado

Base 4 px. Tokens `--s1` (4) a `--s6` (24). Padding de tarjeta estándar 16 px; padding de KPI card 18 / 14 px; padding de header de módulo 16 / 14 / 16 / 4. Gap entre tarjetas en grid: 12 px (4-cols) u 8 px (5-cols).

### 6.5 Backgrounds

- **Página:** color sólido `#F2F3F7` (page-bg). Sin texturas, sin patrones, sin gradientes de fondo.
- **Cards:** blanco puro (`#FFFFFF`).
- **Footer de tarjetas:** `#F9FAFB` (gray-foot).
- **Sidebar / titlebar / statusbar:** blanco. La página es la única superficie gris.
- **Detalle expandible de nota:** `#e9e9e9` (un toque más oscuro que page-bg para diferenciarlo claramente).
- **Sin imágenes de fondo, sin ilustraciones hand-drawn, sin patrones repetidos, sin gradientes en cards.** El único gradiente permitido es el del **logo** (Emerald → Indigo) y del **avatar de usuario** (Violet → Indigo) — y siempre en superficies pequeñas (≤ 64 px).

### 6.6 Animación

| Evento | Propiedad | Duración | Easing |
|--------|-----------|----------|--------|
| Hover botón / card | background, box-shadow | 150 ms | ease |
| Expand nota detail | opacity + translateY | 180 ms | ease |
| Spinner | rotate | 800 ms | linear infinite |
| Sidebar collapse | width | 200 ms | ease-in-out |
| Slide-in notificación | translateX | 250 ms | ease-out |

- **Sin bounces, sin elastic, sin overshoot.** Easing por defecto = `ease` o `ease-in-out`.
- Sin storyboards de entrada para la vista principal. Las vistas aparecen instantáneas.

### 6.7 Estados de interacción

- **Hover:**
  - Botón raised → `box-shadow: Dp2` + background un tono más oscuro.
  - Botón outlined → fondo `rgba(primary, .06)`.
  - Botón flat → fondo `rgba(primary, .08)`.
  - Fila de tabla → `bg #FAFAFA`.
  - Item de sidebar → `bg #F9FAFB` (sin cambiar el icono).
  - Card / KPI → **no hover state** (estáticas; el dato es lo que importa).
- **Press:** no se reduce la escala. El feedback es el cambio de color y la sombra.
- **Focus (inputs):** `border-color: var(--primary)` (#1976D2) + outline removed. Sin glow.
- **Disabled:** `opacity: 0.5` para checks; `bg #BDBDBD color #fff` para botones; `cursor: not-allowed` siempre.
- **Selected (row):** `border-left: 3px solid var(--primary)` + `bg #EEF2FF`.
- **Active (sidebar item):** bg `var(--in-bg)` (#EEF2FF), text e ícono en `var(--in)` (#6366F1), peso 600, `box-shadow: 0 1px 3px rgba(99,102,241,.12)`, `border-radius: var(--r-md)`. Sin border-left.
- **Sidebar item hover:** bg `#F0F4FF`, ícono y texto en `var(--in)`.
- **Sidebar item geometría:** `padding: 8px 12px`, `margin: 1px 8px`, `border-radius: var(--r-md)` (10 px). Las pills viven dentro del sidebar con respiro lateral.

### 6.8 Bordes y radios

- Sin bordes en tarjetas — sólo shadow.
- Bordes 1 px `--gray-line` en titlebar/statusbar/sidebar (separación clara).
- Inputs: borde 1.5 px (resting `--gray-line`, focus `--primary`).
- Chip: sin borde, sólo fondo translúcido.
- Radios: 6 / 10 / 14 / 20 px. Tarjetas KPI usan 20 px; tablas 14 px; chips 12–14 px; botones 10 px.

### 6.9 Sombras

```
--shadow:   0 1px 4px rgba(0,0,0,.08), 0 0 0 .5px rgba(0,0,0,.06)   /* Dp1 */
--shadow-2: 0 2px 6px rgba(0,0,0,.15), 0 1px 3px rgba(0,0,0,.12)    /* Dp2 */
--shadow-3: 0 4px 16px rgba(0,0,0,.16)                              /* Dp3 */
```

Nunca `drop-shadow` en texto. Nunca `text-shadow`. Nunca insets.

### 6.10 Transparencia y blur

- **Chips de pago / estado:** `rgba(color, 0.15)`. Es el único uso sistemático de transparencia.
- **No hay glassmorphism**, no hay `backdrop-filter: blur()` en el sistema. La marca prefiere claridad sólida.

### 6.11 Imagery

- **No hay fotografía** en la UI operativa. El producto es back-office.
- **Avatares** = iniciales sobre fondo gradiente Violet→Indigo (Keorsoft signature).
- **Logo del cliente** (Hotel Misión GDL, etc) aparece sólo como texto en breadcrumb/statusbar — no se trata de marca visual.

### 6.12 Layout rules

- Shell de app fijo: titlebar 48 px (sticky top), statusbar 28 px (sticky bottom), sidebar 220 px (fijo a la izquierda).
- El contenido principal hace su propio scroll; el shell externo nunca scrollea.
- Grid de KPI: 4 columnas iguales (1fr 1fr 1fr 1fr). Reproceso usa 5 columnas para sus KPIs.
- Padding lateral de página: 10–12 px.

---

## 7 · Iconography

**Librería oficial:** **Material Symbols Rounded** (Google Fonts variable, CDN). Se prefiere SIEMPRE la variante **Rounded** sobre Outlined o Sharp — concuerda con los radios 10–20 px del sistema.

```html
<!-- En el head -->
<link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Rounded:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200&display=block" rel="stylesheet" />
<!-- Uso -->
<span class="material-symbols-rounded">restaurant</span>
<!-- Variante rellena ("active") -->
<span class="material-symbols-rounded fill">restaurant</span>
```

- **Por qué Material Symbols (vs Material Icons Round legacy):** Material Symbols es la línea actual de Google — soporta ejes variables (FILL, weight, GRAD, opsz). Permite tener el mismo ícono **outlined** en reposo y **filled** en estados activos sin cambiar de fuente. El sistema mantiene un alias CSS para que código que usa la clase legacy `material-icons-round` siga funcionando contra Material Symbols Rounded.
- **No hay icon font propietario.** Keorsoft no mantiene un sprite ni un set custom.
- **No se usan emoji** en UI. Cero excepciones.
- **Unicode permitido sólo para decoración tipográfica:** `●` (bullet de estado), `›` (breadcrumb separator).

### 7.1 Tamaños

| Tamaño | Uso |
|--------|-----|
| 28 px  | Header de módulo grande (`REPROCESO` icon) |
| 22 px  | KPI cards |
| 20 px  | Caja footer pills, mini-metric pills |
| 18 px  | Sidebar items, botones de barra |
| 16 px  | Botones (con texto) |
| 14–15 px | Chips de pago, breadcrumb chip |
| 12 px  | Statusbar, badges pequeños |

### 7.2 Color y ejes variables

- Default: `font-variation-settings: 'FILL' 0, 'wght' 400, 'GRAD' 0, 'opsz' 24` — outlined, peso normal.
- Variante `.fill`: `'FILL' 1, 'wght' 500` — rellena, peso medio. Úsala para estados activos o iconos decorativos importantes (pill de caja, badge de atención, KPI hero).
- Color por defecto: `--gray-muted`. Activo: color semántico de la sección. Dentro de chip / pill: el color `strong` de su familia sobre el `bg/pale`.

### 7.3 Catálogo de uso (extracto)

| Concepto | Ícono |
|----------|-------|
| Dashboard | `dashboard` |
| Métricas | `bar_chart` |
| Habitaciones | `bed` |
| Registros / login | `login` |
| Reservas | `event` |
| Caja | `account_balance_wallet` |
| Restaurante | `restaurant` |
| Reproceso | `refresh` |
| Facturación | `receipt_long` |
| Reportes | `summarize` |
| Configuración | `settings` |
| Usuarios | `manage_accounts` |
| Buscar | `search` |
| Calendario | `calendar_today` |
| Ordenar | `sort` |
| Expandir/Colapsar | `expand_more` / `expand_less` |
| Check / Ok | `check_circle` |
| Alerta | `error_outline` / `warning` |
| Llegadas | `flight_land` |
| Salidas | `flight_takeoff` |
| Forecast | `pending_actions` |
| Tarjeta | `credit_card` |
| Efectivo | `payments` |
| Transferencia | `swap_horiz` |
| Gasto / Compra | `shopping_bag` |
| Cerrar sesión | `logout` |

Catálogo completo: ver `preview/icons.html` y `reference/SHEDashboard_Design.md` §8.

### 7.5 No-hacer (anti-patterns)

Tomado del spec consolidado (`reference/MeridianUI_Design.md` §15):

- ❌ Gradientes como fondo de página o tarjetas (sólo el icon del logo y el avatar del usuario).
- ❌ Sombras dramáticas o `drop-shadow` en texto.
- ❌ Más de 5 colores en una sola vista.
- ❌ Font-size < 9 px.
- ❌ Animaciones > 300 ms en interacciones de lista.
- ❌ Bordes de tarjeta con color — sólo `shadow Dp1`. Excepción: mini-KPI footer.
- ❌ Íconos sin label en sidebar expandido.
- ❌ Tablas sin sticky thead cuando hay scroll.
- ❌ Botones sin estado disabled cuando hay acción async activa.
- ❌ Mezclar colores semánticos entre módulos (el verde de éxito no es el verde de efectivo).
- ❌ Coco Gothic en cualquier elemento que no sea un wordmark de marca.

### 7.4 Logos y marcas

- `assets/keorsoft-logo.png` — **logo oficial de Keorsoft** (640×640 JPEG con fondo de ondas azules; usar tal cual; recortar a radius 8–14 px en el contexto de un icon-frame).
- `assets/keorsoft-mark-blue.png` — la “ó” aislada en azul Keorsoft sobre transparente, para usos sobre fondo claro.
- `assets/keorsoft-mark-white.png` — la misma “ó” en blanco sobre transparente, para usos sobre superficie de marca / dark.
- `assets/shendevour-logo.svg` / `assets/shendevour-lockup.svg` / `assets/keorsoft-mark.svg` — versiones placeholder anteriores (gradiente Emerald→Indigo + “SH” en Montserrat). **Quedan como referencia histórica** — la marca real es la PNG.

**Construcción:** mark “ó” geométrico en blanco sobre fondo de ondas en azules Keorsoft (`#0E98F8` profundo · `#51B1FB` mid · `#C3D9F0` pale). Tipografía oficial del lettering = **Coco Gothic**. Pista de seguridad alrededor del mark = al menos el alto de la “o”.

---

## 8 · Index del repositorio

| Archivo / carpeta | Qué es |
|-------------------|--------|
| `README.md` | Este documento — la fuente de verdad |
| `SKILL.md` | Skill packaging para Claude Code / agentes |
| `colors_and_type.css` | Tokens: color (5 niveles), tipo, spacing, radios, sombras, transiciones |
| `assets/shendevour-logo.svg` | Marca cuadrada SHEndevour |
| `assets/shendevour-lockup.svg` | Lockup horizontal SHEndevour |
| `assets/keorsoft-mark.svg` | Marca corporativa Keorsoft |
| `preview/colors-semantic.html` | Paleta de 5 niveles (Emerald · Amber · Indigo · Violet · Orange) |
| `preview/colors-neutrals.html` | Grises del sistema + primary |
| `preview/colors-payments.html` | Chips de pago SAT y estado de nota |
| `preview/type-scale.html` | Escala tipográfica |
| `preview/type-display.html` | Montserrat display + tabular nums |
| `preview/radii.html` | Radios `r-sm` a `r-xl` |
| `preview/shadows.html` | Elevación Dp0–Dp3 |
| `preview/spacing.html` | Spacing base 4 px |
| `preview/buttons.html` | Raised / Outlined / Flat / Danger / Icon |
| `preview/inputs.html` | DatePicker outlined + checkboxes |
| `preview/kpi-card.html` | KPI card con accent + segmented bar + legend |
| `preview/table-card.html` | Tabla con header coloreado + footer total |
| `preview/nota-card.html` | Nota row con detalle expandido |
| `preview/sidebar.html` | Sidebar 220 px |
| `preview/titlebar.html` | Titlebar + Content header + Statusbar |
| `preview/caja-footer.html` | Caja footer 4-up |
| `preview/mini-kpis.html` | Mini-KPI strips (verde / naranja / azul) |
| `preview/icons.html` | Catálogo Material Icons Round |
| `preview/logo.html` | Logos SHEndevour + Keorsoft |
| `ui_kits/shendevour-web/index.html` | **Prototipo click-through** del producto web — Dashboard + Reproceso |
| `ui_kits/shendevour-web/components/*.jsx` | Componentes React reutilizables |
| `ui_kits/shendevour-web/tokens.css` | Copia local de los tokens |
| `ui_kits/shendevour-web/styles.css` | Estilos del shell + buttons + chips |
| `reference/*.md` y `reference/*.html` | Specs originales (read-only) |

> **Consejo:** para empezar un mock nuevo, copia `colors_and_type.css` al lado de tu HTML, `<link>` a Material Icons Round desde el head, y reusa los JSX de `ui_kits/shendevour-web/components/`.
