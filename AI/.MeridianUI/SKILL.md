---
name: meridianui-design
description: Use this skill to generate well-branded interfaces and assets for Keorsoft (and its product line, e.g. SHEndevour) using the MeridianUI design language — either for production code or throwaway prototypes / mocks / decks. Contains essential design guidelines, color palette, typography, fonts, icon catalog and full UI-kit components for prototyping.
user-invocable: true
---

# MeridianUI · Design skill

Read **README.md** at the root of this skill — it is the source of truth for tone, content rules, color, typography, spacing, animation, iconography and the UI-kit catalogue.

Then explore:

- `colors_and_type.css` — every CSS variable in the system. Copy this file into any artifact and you have all tokens.
- `preview/*.html` — small, focused specimen cards showing each pattern (palettes, typography, buttons, KPI card, table card, nota card, sidebar, titlebar, caja footer, icons, logo).
- `ui_kits/shendevour-web/` — full click-through prototype of the **SHEndevour Web** product, with JSX components broken into small files (`Atoms.jsx`, `Shell.jsx`, `Dashboard.jsx`, `Reproceso.jsx`). Use these as starting components — they cover sidebar, titlebar, statusbar, KPI cards, table cards, payment chips, nota rows with expandable detail, and the search bar / sort bar pattern.
- `assets/` — logos in SVG (Emerald → Indigo gradient mark).
- `reference/` — the original design specs (`SHEDashboard_Design.md`, `MetricsDashboard_Design.md`, `AccountStateDashboard_Design.md`) — densest source of patterns and bindings.

## When this skill is invoked

If the user invokes the skill **without other guidance**, ask:

1. What are they building? A throwaway prototype / mock, an internal slide, or production code?
2. Which product / module? (`SHEndevour Web` is the canonical target; otherwise treat it as a new module that should fit alongside the existing ones.)
3. Spanish content (default) or English?
4. Any tweakable parameters they want exposed?

## Working rules

- **Always start from `colors_and_type.css`.** Import it (or inline its tokens) before writing any CSS. Never invent new hex values — use the 5-step semantic ramp (`em / am / in / vi / or`) and neutrals.
- **Use Material Symbols Rounded** for iconography (Google Fonts variable font). Add both `<link>` and `<span class="material-symbols-rounded">name</span>`. For active/filled states, add the `fill` class. Material Icons Round (legacy) still works as an alias. Never hand-roll SVG icons and never use emoji.
- **Spanish (es-MX) by default.** Title Case for product titles, UPPERCASE labels with letter-spacing for metas / KPI labels, sentence case for body and buttons. No emoji.
- **Tone: enterprise-soft.** Functional, dense, no marketing voice. No exclamations.
- **Visual rules to never break:**
  - No page-background gradients. No textured backgrounds. Page bg is `#F2F3F7`, cards are white.
  - Card borders are shadow (`Dp1`), never colored — exception: mini-KPI footer strips.
  - Animations ≤ 200 ms, easing `ease` or `ease-in-out`. No bounce, no overshoot.
  - Hover states are subtle: bg shift, no scale, no rotation.
  - Tabular nums (`font-variant-numeric: tabular-nums`) on every monetary figure.
- **Layout for desktop apps:** titlebar 48 px (sticky top) + sidebar 220 px (fixed) + statusbar 28 px (sticky bottom). Content scrolls in the middle.
- **Card radii:** 20 px for KPI/caja, 14 px for tables, 10 px for buttons/chips, 6 px for inputs.

## Artifact production

- **HTML mocks / prototypes:** copy `colors_and_type.css` next to the file and `<link>` it. Use the JSX components in `ui_kits/shendevour-web/components/` as direct copy-paste starting points (they auto-register on `window`). The full shell renders with `<Titlebar />`, `<Sidebar />`, `<main className="main">…</main>`, `<Statusbar />` inside a `<div className="shell">`.
- **Slides:** use Montserrat 22–28 px for titles, Segoe UI / Inter for body. Emerald `#10B981` as accent. Stick to the 5-step semantic palette for any data viz.
- **Production code:** the tokens map cleanly to either `MaterialDesignInXaml` `DynamicResource` keys (for WPF) or CSS custom properties (for web). Document any mapping in code comments so the back-end stack can mirror it.

## Caveats / flags

- **Segoe UI is proprietary** (Microsoft). On the web, the system falls back to **Inter** (Google Fonts). If the user wants exact Segoe UI on the web, they must supply a licensed `.ttf` and add it to `fonts/` with an `@font-face` declaration.
- **Logos** in `assets/` are SVG reconstructions of the in-product mark (gradient + Montserrat 800 SH lettering). If official master logos exist, replace them.
- **No live Figma or GitHub** source for this brand at the moment — everything is derived from the WPF spec docs and HTML previews under `reference/`. Always ask if the user has newer source-of-truth files.
