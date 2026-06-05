# Plan de Desarrollo e Implementación: OctagramDelivery

**Objetivo:** Desarrollar una Aplicación Web PWA responsiva y rápida para la gestión y supervisión de repartidores, multi-negocio, con roles jerárquicos y cálculos en tiempo real de entregas, devoluciones y dinero en efectivo.

**Tecnologías:**
- **Frontend:** Blazor WebAssembly (C#) configurado como PWA.
- **UI:** Integración del sistema de diseño propietario **MeridianUI** (CSS nativo/Custom variables).
- **Backend:** ASP.NET Core Web API (C#) + SignalR (para tiempo real si es requerido).
- **Base de Datos:** SQL Server (Entity Framework Core).
- **Infraestructura:** Ubuntu Server, Docker, Nginx Proxy Manager, Cloudflare Tunnel.

---

## 🛠️ Fase 1: Arquitectura y Diseño de Datos

### 1. Modelado de Base de Datos (SQL Server)
- **Negocios y Sucursales:** Gestión multi-tenant para aislar la información por negocio.
- **Usuarios y Roles:**
  - `Admin`: Control total del sistema, todos los negocios.
  - `Gerente`: Control sobre un grupo de negocios asignados.
  - `Supervisor`: Control exclusivo sobre 1 negocio.
  - `Repartidor`: Acceso solo a sus rutas y clientes del día.
- **Catálogo de Clientes:** Días de entrega programados, horarios aproximados.
- **Catálogo de Productos:** 
  - Venta por Pieza o por Gramaje.
  - Gramajes rápidos (1/4KG, 1/2KG, 1KG) y Personalizado.
  - Precios asignados por producto.
- **Sistema de Repartos (Core):**
  - Entidad `Jornada/Dia` por Repartidor.
  - Entidad `Vueltas/Rondas` (Dinámicas: 1, 2, 3...).
  - Entidad `DetalleReparto` (Producto, Cantidad Entregada, Devoluciones, Método de Pago).

### 2. Diseño de UI/UX
- Configuración inicial de Blazor integrando `colors_and_type.css` de **MeridianUI**.
- Diseño "Mobile-First" pensando en el Repartidor (pantallas pequeñas) y "Desktop" para los Dashboards de Supervisión.

---

## ⚙️ Fase 2: Desarrollo del Backend (ASP.NET Core API)

### 1. Autenticación y Autorización
- Implementación de JWT (JSON Web Tokens) con políticas estrictas por Rol (Admin, Gerente, Supervisor, Repartidor).

### 2. APIs de Gestión (CRUDs)
- Endpoints para gestión de Negocios, Empleados, Clientes y Productos.
- Asignación de rutas y carteras de clientes a repartidores.

### 3. Motor de Cálculos y Transacciones
- Lógica centralizada para calcular: Totales de venta, totales devueltos (piezas/gramos y dinero), dinero efectivo vs otros métodos de pago.
- Endpoints de actualización rápida (Bulk Update) para que la vista de "hoja de cálculo" del repartidor guarde sin demoras.

---

## 📱 Fase 3: PWA del Repartidor (Frontend Core)

### 1. Vista de Hoja de Cálculo Interactiva
- Grid optimizada (tipo Excel/Spreadsheet) donde cada fila es un Cliente y las columnas representan las Vueltas/Rondas.
- **Rondas Dinámicas:** Botón para "Agregar Vuelta" que replica la interfaz de captura para la nueva ronda.
- **Captura Rápida:** Inputs para cantidad entregada, devoluciones por producto.
- **Manejo de Gramajes:** Selector ágil para 1/4KG, 1/2KG, 1KG o input libre.

### 2. Panel de Totales en Tiempo Real
- **Cálculos Dinámicos en Cliente (Blazor):** Sumatorias por vuelta, totales del día, devoluciones totales y por producto.
- **Exclusiones de Efectivo:** Checkbox interactivo en cada cliente para "Descartar de total en efectivo" (ej. pagos con tarjeta o transferencia), reflejando visualmente el descuento, pero manteniendo el registro de la venta.
- **Gran Total:** El valor exacto final que el repartidor debe entregar (Efectivo a rendir).

### 3. Funcionalidad PWA
- Configuración del Service Worker para caché de assets, permitiendo instalación en Android/iOS como aplicación nativa.
- Sincronización básica de estado en caso de pérdida intermitente de señal.

---

## 📊 Fase 4: Dashboards de Supervisión (Admin, Gerente, Supervisor)

### 1. Panel de Supervisor (1 Negocio)
- Monitoreo en vivo de los repartidores activos.
- Cuadre de caja esperado vs reportado.
- Inventario en calle (Entregado vs Devuelto).

### 2. Panel de Gerente (Múltiples Negocios)
- Resumen agregado de todos sus negocios a cargo.
- Comparativas de ventas por negocio y rendimiento de repartidores.

### 3. Panel de Administrador (Global)
- Gestión del sistema completo.
- Creación de nuevos Negocios y asignación de Gerentes.
- Auditoría global del flujo de dinero y productos.

---

## 🚀 Fase 5: Despliegue en Ubuntu Server (Docker)

Basado en la infraestructura `GuiaUbuntuServer.md` actual:

### 1. Preparación de Contenedores
- **SQL Server:** Contenedor `mcr.microsoft.com/mssql/server` persistiendo datos en un volumen local.
- **ASP.NET API:** `Dockerfile` multi-stage compilando el backend y exponiéndolo (ej. puerto 8080).
- **Blazor PWA:** Compilada estáticamente y servida mediante un contenedor Nginx (`nginx:alpine`).

### 2. Configuración de Red Proxy Global
- Todos los servicios web y API se conectarán a la red externa `proxy`.

### 3. Configuración de Dominios (Nginx Proxy Manager & Cloudflare)
- **Dominio Base:** `endevour.mx`
- **Subdominios propuestos (1er nivel para SSL gratuito):**
  - `octagram-api.endevour.mx` (Apunta al contenedor ASP.NET).
  - `octagram-app.endevour.mx` (Apunta al contenedor Nginx estático con la PWA).
- Configuración de certificados SSL automáticos y Force SSL en NPM.
- Rutas en el `config.yml` del Cloudflare Tunnel para rutear el tráfico hacia los subdominios.

---
*Este documento establece la ruta técnica para OctagramDelivery. Validado este plan, se procederá a la creación de la base de datos y la inicialización de los proyectos en C#.*
