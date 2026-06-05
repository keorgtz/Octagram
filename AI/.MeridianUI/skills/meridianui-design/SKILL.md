---
name: meridianui-design
description: Use this skill to generate well-branded interfaces and assets for Keorsoft (and its product line, e.g. SHEndevour) using the MeridianUI design language — for HTML/CSS, Blazor Web, WPF/XAML or .NET MAUI. Contains essential design guidelines, color palette, typography, fonts, icon catalog and full UI-kit components for any UI platform.
user-invocable: true
tools: Read, Glob
---

# MeridianUI · Design Skill — Multi-Platform v1.1

Read `C:\Users\kevin\.MeridianUI\README.md` first — source of truth for tone, content rules, color, typography, spacing, animation, iconography and UI-kit catalogue.

## Files by platform

| Plataforma     | Token file                   | Components / Kit                                  | Platform guide                        |
|----------------|------------------------------|---------------------------------------------------|---------------------------------------|
| **HTML / CSS** | `colors_and_type.css`        | `ui_kits/shendevour-web/components/*.jsx`         | (this file)                           |
| **Blazor Web** | `colors_and_type.css`        | `ui_kits/shendevour-blazor/*.razor`               | `references/platform-blazor.md`       |
| **WPF / XAML** | `tokens-wpf.xaml`            | `reference/MetricsDashboard_Design.md`, `reference/AccountStateDashboard_Design.md` | `references/platform-wpf.md` |
| **.NET MAUI**  | `tokens-maui.xaml`           | (usa los mismos tokens, construye controles custom) | `references/platform-maui.md`       |

All paths relative to: `C:\Users\kevin\.MeridianUI\`

## When this skill is invoked

If invoked **without other guidance**, ask:
1. ¿Qué están construyendo? (prototipo desechable / mock, slide interno, o código de producción)
2. ¿Qué plataforma? (HTML/CSS · Blazor · WPF · MAUI · otro)
3. ¿Qué producto / módulo? (`SHEndevour Desktop` o `SHEndevour Web` son el target canónico)
4. ¿Contenido en español (default) o inglés?

## Platform dispatch

Antes de generar cualquier código, **detecta la plataforma** e impórtala:

- Si es **HTML / CSS / Prototype**: importar `colors_and_type.css`, seguir reglas de la sección HTML de este skill.
- Si es **Blazor / Razor**: leer `references/platform-blazor.md` y usar los `.razor` de `ui_kits/shendevour-blazor/`.
- Si es **WPF / XAML**: leer `references/platform-wpf.md` y el token file `tokens-wpf.xaml`.
- Si es **MAUI / .NET Multi-platform**: leer `references/platform-maui.md` y el token file `tokens-maui.xaml`.

## Working rules — ALL platforms

- **Always start from the token file** de la plataforma. Nunca inventar valores hex — usar la paleta semántica de 5 pasos (`em / am / in / vi / or`) y los neutros.
- **Iconografía: Material Symbols Rounded** (Google Fonts variable). En web: `<span class="material-symbols-rounded">name</span>`. En Blazor: `<MIcon Name="name" />`. En WPF/MAUI: usar el carácter unicode de Material Symbols o la fuente TTF. Nunca hand-roll SVG icons. Nunca emojis.
- **Español (es-MX) por defecto.** UPPERCASE para módulos y labels, Sentence case para botones y body. Sin emojis. Sin exclamaciones.
- **Tono: enterprise-soft.** Funcional, denso, sin marketing voice.
- **Reglas visuales que nunca se rompen:**
  - Sin gradientes de fondo de página. Sin texturas. `page-bg = #F2F3F7`, cards = blanco.
  - Bordes de tarjeta: shadow Dp1, nunca coloreados. Excepción: mini-KPI footer strips.
  - Animaciones ≤ 200 ms. Sin bounce. Easing: `ease` o `ease-in-out`.
  - Hover suaves: solo cambio de fondo/sombra. Sin escala. Sin rotación.
  - Importes monetarios: siempre tabular-nums.
- **Layout desktop (WPF / Blazor desktop):** titlebar 48 px (sticky top) + sidebar 220 px (fijo) + statusbar 28 px (sticky bottom).
- **Radios:** 20 px KPI/caja · 14 px tables · 10 px buttons/chips · 6 px inputs.

## HTML / CSS / Prototype rules

- Copiar `colors_and_type.css` al lado del HTML y `<link>`-lo.
- Usar los JSX de `ui_kits/shendevour-web/components/` como punto de partida.
- Shell: `<div class="shell">` con `<Titlebar />`, `<Sidebar />`, `<main class="main">…</main>`, `<Statusbar />`.
- Ver `preview/*.html` para specimens de cada componente.

## Artifact production

- **HTML mocks:** copiar `colors_and_type.css`, usar JSX de `ui_kits/shendevour-web/components/`.
- **Blazor:** ver `references/platform-blazor.md`.
- **Slides:** Montserrat 22–28 px para títulos, Inter / Segoe UI para body. Emerald `#10B981` como acento.
- **WPF/MAUI producción:** ver la referencia de plataforma correspondiente. Los tokens mapean limpiamente a `DynamicResource` keys (WPF) o `StaticResource` (MAUI).

## Caveats

- **Segoe UI** es propietaria (Microsoft). En web cae a Inter. Para WPF desktop está disponible en sistema.
- **Coco Gothic** es la fuente de wordmarks de marca — solo para los títulos de producto (SHEndevour, Keorsoft). NO aparece en UI corriente. Entregar como asset PNG/SVG.
- Los logos en `assets/` son reconstrucciones SVG. Si existen archivos master oficiales, usarlos en su lugar.
