# SHEndevour Web · UI Kit

Recreación interactiva del producto **SHEndevour Web** usando MeridianUI.

## Vistas

- **Dashboard** (`Resumen del Día`) — KPI cards con barra segmentada, 4 tablas con header coloreado, caja footer 4-up.
- **Reproceso** — barra de búsqueda con date-pickers, KPI row de 5, lista de notas con detalle expandible, footer con mini-KPIs y botón destructivo.
- **Restaurante** — rack de mesas con sillas (zonas Salón principal + Terraza), 4 mini-KPIs de operación, action bar con búsqueda + acciones rápidas (Nuevo pedido / Reservar / Combinar mesas / Filtros), toggle Mesas ↔ Lista, y **dock inferior fijo** de pedidos rápidos (8 cards: mesa, cliente, ítems, total, tiempo, status). Estados de mesa color-coded: Libre · Ocupada · Reservada · Limpiando · Atención.

Otros módulos del sidebar muestran placeholder con `construction` icon — los componentes existentes cubren los patrones que harían falta.

## Estructura

```
shendevour-web/
├── index.html                ← prototipo
├── tokens.css                ← copia local de colors_and_type.css
├── styles.css                ← shell + buttons + chips
├── assets/
│   ├── shendevour-logo.svg
│   └── shendevour-lockup.svg
└── components/
    ├── Atoms.jsx             ← MIcon (Material Symbols Rounded), Chip, Checkbox, money()
    ├── Shell.jsx             ← Titlebar, Sidebar, Statusbar, ContentHeader
    ├── Dashboard.jsx         ← KpiCard, TableCard, CajaFooter, DashboardView
    ├── Reproceso.jsx         ← NotaRow, ReprocesoView
    └── Restaurant.jsx        ← RestFloorTable, RestOrderCard, RestMiniMetric, RestaurantView
```

## Cómo arranca el prototipo

`index.html` carga React + Babel via CDN, importa los 4 archivos JSX y monta `<App>`. Cada componente exporta a `window` al final de su archivo para que `index.html` los pueda consumir desde un script Babel separado.

## Comportamiento

- Click en sidebar cambia vista (Dashboard ↔ Reproceso ↔ placeholder).
- Botón **Actualizar** activa el spinner por 900 ms.
- En Reproceso: checkboxes seleccionan notas (las facturadas están deshabilitadas), los botones **Todas / Ninguna** funcionan, el chevron de cada nota expande el detalle, los KPIs y el footer se recalculan en vivo.
- Los botones de ordenamiento alternan asc/desc (visual; no se reordena la lista).

## No incluido (por diseño)

- Lógica real de carga, ordenamiento numérico, ejecución del reproceso.
- Persistencia. Cada refresh reinicia el estado seed.
- Módulos placeholder (Habitaciones, Reservas, Caja, etc.) — se demuestra el patrón con los dos casos límite (lectura densa y operación con selección masiva).
