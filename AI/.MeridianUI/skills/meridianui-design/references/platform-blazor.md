# MeridianUI · Platform Guide — Blazor Web

## Setup

### 1. Incluir tokens CSS en el HTML base

En `App.razor` o `index.html` / `_Host.cshtml`:

```html
<head>
    <!-- Tokens MeridianUI -->
    <link rel="stylesheet" href="_content/TuApp/css/meridian/colors_and_type.css">
    <!-- O inline si es un proyecto único -->
    <link rel="stylesheet" href="css/colors_and_type.css">

    <!-- Material Symbols Rounded — iconografía -->
    <link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Rounded:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200&display=block" rel="stylesheet"/>

    <!-- Inter desde Google Fonts -->
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap" rel="stylesheet"/>
</head>
```

### 2. Clase base en body o contenedor raíz

```html
<body class="meridian-base">
```

### 3. Agregar componentes de `ui_kits/shendevour-blazor/`

Copiar los archivos `.razor` a la carpeta `Components/MeridianUI/` del proyecto Blazor y agregar el namespace en `_Imports.razor`:

```razor
@using TuApp.Components.MeridianUI
```

## Componentes disponibles

| Componente         | Archivo              | Uso principal                              |
|--------------------|----------------------|--------------------------------------------|
| `<MIcon>`          | `MIcon.razor`        | Ícono Material Symbols Rounded             |
| `<KpiCard>`        | `KpiCard.razor`      | Tarjeta KPI con accent bar + barra segm.   |
| `<PageHeader>`     | `PageHeader.razor`   | Header de módulo con título + acciones     |
| `<Shell>`          | `Shell.razor`        | Layout shell: titlebar + sidebar + main    |

## Example: MIcon

```razor
<!-- Ícono básico -->
<MIcon Name="dashboard" />

<!-- Ícono relleno (estado activo) -->
<MIcon Name="restaurant" Fill="true" Size="22" />

<!-- Ícono con color semántico -->
<MIcon Name="check_circle" Color="var(--em)" Size="18" />
```

## Example: KpiCard

```razor
<KpiCard
    Label="LLEGADAS"
    Value="@llegadas.ToString()"
    ValueColor="var(--em)"
    BadgePct="+12%"
    BadgeColor="var(--em)"
    BadgeBg="var(--em-pl)"
    SegmentFill="0.7"
    SegmentColor="var(--em)"
    SegmentBgColor="var(--em-lt)"
    Legend1="Check-in"
    Legend2="Por llegar">
</KpiCard>
```

## Example: Shell layout completo

```razor
@* MainLayout.razor *@
<Shell HotelName="Hotel Misión GDL"
       ModuleName="Dashboard"
       UserName="J. Rodríguez"
       TurnoName="Matutino (08:00 – 16:00)"
       AppVersion="4.1.3"
       NavItems="@navItems">
    <ChildContent>
        @Body
    </ChildContent>
</Shell>
```

## Example: PageHeader

```razor
<PageHeader Title="RESUMEN DEL DÍA"
            Subtitle="@DateTime.Today.ToString("dddd, d 'de' MMMM 'de' yyyy", new CultureInfo("es-MX"))">
    <Actions>
        <button class="btn-raised">Actualizar</button>
    </Actions>
</PageHeader>
```

## CSS classes disponibles (de colors_and_type.css)

```html
<!-- Tipografía semántica -->
<h1 class="h-page-title">Título de página</h1>
<h2 class="h-section-title">Sección</h2>
<span class="t-kpi-value">42</span>
<span class="t-label">LLEGADAS</span>
<span class="t-meta">A REPROCESAR</span>
<p class="t-body">Texto de cuerpo</p>
<span class="t-amount">$1,234.50</span>
```

## Blazor-specific rules

- **Siempre importar `colors_and_type.css` antes** de cualquier otro CSS de la app.
- **Material Symbols Rounded:** cargar desde Google Fonts CDN en el `<head>`. No instalar como npm — la fuente variable es más ligera desde CDN.
- **Importes monetarios:** aplicar clase `t-amount` (ya tiene `font-variant-numeric: tabular-nums`).
- **Chip de pago:** usar las variables `--pay-efec`, `--pay-tcred`, etc. con `rgba(color, 0.15)` de fondo.
- **Sin glassmorphism, sin backdrop-filter.** No agregar `backdrop-filter: blur()` en ningun componente.
- **Componentes stateless:** los componentes razor de MeridianUI son presentacionales. La lógica de negocio va en los ViewModels/Services de la página que los usa.
- **CascadingValue para tema:** si se necesita cambio de tema (light/dark), usar `CascadingValue` con la clase CSS en el `<Shell>`.
