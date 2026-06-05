# Guía de Integración del Licenciador API — SHEndevour Desktop

Versión: 4.0 · Modelo: validación online con fallback offline + EmpresaId como identificador permanente

---

## Arquitectura de validación

```
App WPF — Login
    │
    ├─ Generar CodigoInstalacion  (MachineGuid → sum estable → 8 chars A-Z)
    ├─ EmpresaId                  (config local — el usuario lo ingresa la primera vez, dado por el admin)
    │
    ├─ ¿API disponible?
    │       │
    │    SÍ └─ POST /api/licencia/obtener  {EmpresaId, CodigoInstalacion}
    │              │
    │              ├─ 200 OK  (licencia encontrada en BD)
    │              │       ├─ Verificar CodigoInstalacion == hardware actual  (seguridad)
    │              │       ├─ Verificar EmpresaId coincide con el storage     (seguridad)
    │              │       ├─ Verificar FechaLicencia >= hoy                  (vigencia)
    │              │       ├─ Actualizar storage local con todos los datos
    │              │       └─ PERMITIR acceso
    │              │
    │              └─ 404 Not Found → BLOQUEAR (equipo sin licencia asignada)
    │
    └─ NO (API caída) → Validar con datos locales
                │
                ├─ ¿Hay datos locales?  No → BLOQUEAR
                ├─ ¿Licencia expirada? Sí → BLOQUEAR
                ├─ ¿CodigoInstalacion coincide? No → BLOQUEAR
                └─ TODO OK → PERMITIR (notificar modo sin conexión)
```

> **Compatibilidad legacy:** El endpoint también acepta `{CodigoInstalacion, RFC}` si no se dispone de `EmpresaId` (instalaciones antiguas).

---

## Paso 1 — Generar el Código de Instalación

Siempre usa el **mismo** algoritmo en toda la vida del producto.  
Usa `Sum(c => c)` en lugar de `GetHashCode()` (estable entre runtimes y versiones de .NET).

```csharp
using Microsoft.Win32;

public static class MachineHelper
{
    private static string GetMachineGuid()
    {
        using var key = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Cryptography");
        return key?.GetValue("MachineGuid")?.ToString() ?? "UNKNOWN";
    }

    /// <summary>
    /// Código único y estable derivado del MachineGuid Windows.
    /// Nunca cambia mientras no se reinstale Windows.
    /// </summary>
    public static string GetCodigoInstalacion()
    {
        string machineGuid = GetMachineGuid();

        // Suma de chars alfanuméricos — estable entre runtimes
        int seed = machineGuid
            .Where(char.IsLetterOrDigit)
            .Sum(c => c);

        var rng = new Random(seed);
        var code = new char[8];
        for (int i = 0; i < 8; i++)
            code[i] = (char)rng.Next(65, 91); // A-Z

        return new string(code);
    }
}
```

> **CRÍTICO:** Si este algoritmo cambia después del primer despliegue, todos los equipos ya
> activados generarán un `CodigoInstalacion` distinto y la API responderá `HardwareInvalido`.

---

## Paso 2 — Almacenamiento Local (`LicenciaLocal`)

```csharp
using Microsoft.Win32;
using System.Text.Json;

public class LicenciaLocal
{
    // Inmutables — se establecen en la primera activación y NUNCA se sobreescriben
    public string CodigoInstalacion { get; set; } = string.Empty;
    public string EmpresaId         { get; set; } = string.Empty; // Identificador permanente del hotel

    // Mutables — se actualizan en cada sincronización exitosa con la API
    public string RFC              { get; set; } = string.Empty;
    public string ClaveLicencia    { get; set; } = string.Empty;
    public string ClaveInstalacion { get; set; } = string.Empty;
    public DateTime FechaLicencia  { get; set; }
    public string NombreComercial  { get; set; } = string.Empty;
    public string RazonSocial      { get; set; } = string.Empty;
    public string NombreCliente    { get; set; } = string.Empty;
    public DateTime UltimaSincronizacion { get; set; }
}

public static class LicenciaStorage
{
    private const string RegPath = @"SOFTWARE\SHEndevour\Licensing";

    public static LicenciaLocal? Leer()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegPath);
            if (key == null) return null;
            var json = key.GetValue("LicenciaData")?.ToString();
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonSerializer.Deserialize<LicenciaLocal>(json);
        }
        catch { return null; }
    }

    /// <summary>
    /// NUNCA sobreescribe CodigoInstalacion ni EmpresaId si ya están establecidos.
    /// </summary>
    public static void Guardar(LicenciaLocal nueva, LicenciaLocal? existente)
    {
        var guardar = existente ?? new LicenciaLocal();

        if (string.IsNullOrWhiteSpace(guardar.CodigoInstalacion))
            guardar.CodigoInstalacion = nueva.CodigoInstalacion;
        if (string.IsNullOrWhiteSpace(guardar.EmpresaId))
            guardar.EmpresaId = nueva.EmpresaId;

        guardar.RFC                  = nueva.RFC;
        guardar.ClaveLicencia        = nueva.ClaveLicencia;
        guardar.ClaveInstalacion     = nueva.ClaveInstalacion;
        guardar.FechaLicencia        = nueva.FechaLicencia;
        guardar.NombreComercial      = nueva.NombreComercial;
        guardar.UltimaSincronizacion = DateTime.UtcNow;

        var json = JsonSerializer.Serialize(guardar);
        using var key = Registry.CurrentUser.CreateSubKey(RegPath);
        key.SetValue("LicenciaData", json);
    }

    public static void Borrar()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegPath, writable: true);
        key?.DeleteValue("LicenciaData", throwOnMissingValue: false);
    }
}
```

---

## Paso 3 — Prueba de Conexión

```csharp
public class LicenseService
{
    private readonly HttpClient _http;
    private readonly string _apiBase;
    private readonly string _apiKey;

    public LicenseService(string apiBaseUrl, string apiKey)
    {
        _apiBase = apiBaseUrl.TrimEnd('/');
        _apiKey  = apiKey;
        _http    = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var resp = await _http.GetAsync($"{_apiBase}/api/health");
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
```

---

## Paso 4 — Obtener licencia de la API (online)

```
POST /api/licencia/obtener
Header: X-Api-Key: <tu-api-key>
Content-Type: application/json
```

**Request (nuevo — EmpresaId + CodigoInstalacion):**
```json
{
  "empresaId":         "SHE-A1B2C3D4",
  "codigoInstalacion": "QWERTYUI"
}
```

**Request alternativo — solo EmpresaId (si el equipo tiene una sola instalación):**
```json
{
  "empresaId": "SHE-A1B2C3D4"
}
```

**Request legacy — CodigoInstalacion + RFC (compatibilidad con versiones anteriores):**
```json
{
  "codigoInstalacion": "QWERTYUI",
  "rfc":               "XAXX010101000"
}
```

**Response 200 OK (licencia encontrada):**
```json
{
  "empresaId":         "SHE-A1B2C3D4",
  "codigoInstalacion": "QWERTYUI",
  "claveInstalacion":  "XJDKQWPE",
  "claveLicencia":     "PLMKQWAZ",
  "fechaLicencia":     "2027-05-26T00:00:00",
  "rfc":               "XAXX010101000",
  "razonSocial":       "Mi Empresa S.A. de C.V.",
  "nombreComercial":   "Mi Empresa",
  "nombreCliente":     "Juan Pérez"
}
```

**Response 404 Not Found:**
```json
{ "mensaje": "No se encontró licencia activa." }
```

**Response 400 Bad Request:**
```json
{ "mensaje": "Proporciona EmpresaId o CodigoInstalacion+RFC." }
```

La API devuelve los datos tal como están en la BD. La validación criptográfica (¿coincide `ClaveLicencia` con el algoritmo?) ocurre en la app WPF con los datos recibidos.

```csharp
private record ObtenerLicenciaRequest(
    string? EmpresaId,
    string? CodigoInstalacion,
    string? RFC = null);

private record ObtenerLicenciaResponse(
    string EmpresaId, string CodigoInstalacion, string ClaveInstalacion, string ClaveLicencia,
    DateTime FechaLicencia, string RFC,
    string RazonSocial, string NombreComercial, string NombreCliente);

public async Task<ObtenerLicenciaResponse?> ObtenerLicenciaOnlineAsync(
    string empresaId, string codigoInstalacion)
{
    var req = new HttpRequestMessage(HttpMethod.Post, $"{_apiBase}/api/licencia/obtener");
    req.Headers.Add("X-Api-Key", _apiKey);
    req.Content = JsonContent.Create(new ObtenerLicenciaRequest(empresaId, codigoInstalacion));
    try
    {
        var resp = await _http.SendAsync(req);
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<ObtenerLicenciaResponse>();
    }
    catch { return null; }
}
```

---

## Paso 5 — Validar sin conexión (offline)

```csharp
public static (bool Valida, string Mensaje) ValidarOffline(string codigoInstalacionMaquina)
{
    var local = LicenciaStorage.Leer();
    if (local == null)
        return (false, "No hay licencia activada en este equipo.");
    if (local.FechaLicencia < DateTime.Today)
        return (false, $"La licencia expiró el {local.FechaLicencia:dd/MM/yyyy}.");
    if (local.CodigoInstalacion != codigoInstalacionMaquina)
        return (false, "La licencia almacenada no corresponde a este equipo.");
    return (true, string.Empty);
}
```

---

## Paso 6 — Flujo completo en el Login

```csharp
public async Task<bool> VerificarLicenciaAlLoginAsync()
{
    string codigoMaquina = MachineHelper.GetCodigoInstalacion();
    LicenciaLocal? localActual = LicenciaStorage.Leer();

    // Primera vez: pedir EmpresaId al usuario (el admin se lo proporciona)
    string? empresaId = localActual?.EmpresaId;
    if (string.IsNullOrWhiteSpace(empresaId))
    {
        empresaId = MostrarDialogoEmpresaId(codigoMaquina); // muestra CodigoInstalacion, pide EmpresaId
        if (string.IsNullOrWhiteSpace(empresaId)) return false;
    }

    if (await TestConnectionAsync())
    {
        var r = await ObtenerLicenciaOnlineAsync(empresaId, codigoMaquina);

        if (r == null)
        {
            MostrarError("No existe licencia activa para este equipo. " +
                         $"Proporciona al administrador el código: {codigoMaquina}");
            return false;
        }

        // Verificaciones de seguridad
        if (r.CodigoInstalacion != codigoMaquina)
        {
            MostrarError("Inconsistencia de hardware. Contacta a tu proveedor.");
            return false;
        }
        if (!string.Equals(r.EmpresaId, empresaId, StringComparison.OrdinalIgnoreCase))
        {
            MostrarError("El EmpresaId no coincide con la licencia registrada.");
            return false;
        }
        if (r.FechaLicencia < DateTime.Today)
        {
            MostrarError($"La licencia venció el {r.FechaLicencia:dd/MM/yyyy}. " +
                         "Renuévala con tu proveedor.");
            return false;
        }

        // Guardar todos los datos recibidos
        LicenciaStorage.Guardar(new LicenciaLocal
        {
            EmpresaId         = r.EmpresaId,
            CodigoInstalacion = r.CodigoInstalacion,
            RFC               = r.RFC,
            ClaveLicencia     = r.ClaveLicencia,
            ClaveInstalacion  = r.ClaveInstalacion,
            FechaLicencia     = r.FechaLicencia,
            NombreComercial   = r.NombreComercial,
            RazonSocial       = r.RazonSocial,
            NombreCliente     = r.NombreCliente
        }, localActual);

        if (r.FechaLicencia <= DateTime.Today.AddDays(30))
            MessageBox.Show($"Tu licencia vence el {r.FechaLicencia:dd/MM/yyyy}.",
                "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);

        return true;
    }

    // Fallback offline
    var (valida, mensaje) = ValidarOffline(codigoMaquina);
    if (!valida) { MostrarError("Sin conexión.\n" + mensaje); return false; }
    MessageBox.Show(
        $"Modo sin conexión. Licencia válida hasta {localActual!.FechaLicencia:dd/MM/yyyy}.",
        "Modo sin conexión", MessageBoxButton.OK, MessageBoxImage.Information);
    return true;
}
```

---

## Paso 7 — Diálogo de Activación (primera vez)

| Campo | Descripción |
|-------|-------------|
| **Código de instalación** | Solo lectura — el usuario lo comunica al administrador para que registre la licencia |
| **EmpresaId** | Input donde el usuario ingresa el `EmpresaId` asignado por el administrador (ej. `SHE-A1B2C3D4`). Se guarda inmutable en el storage. |

> El administrador registra la licencia en el panel web, obtiene el `EmpresaId` del cliente (visible en el listado y en el detalle del cliente con botón copiar), y se lo proporciona al hotel para que lo ingrese en el diálogo de activación.

> **Migración desde versiones con RFC:** si el WPF aún no tiene `EmpresaId` en el storage, mostrar el diálogo de activación nuevamente para que el usuario ingrese el `EmpresaId`. El endpoint legacy `{CodigoInstalacion, RFC}` sigue activo para la transición.

---

## Paso 8 — Configuración del cliente WPF

```xml
<!-- App.config -->
<appSettings>
  <add key="LicenseApiUrl" value="https://tu-servidor.com:5001" />
  <add key="LicenseApiKey" value="TU_CLAVE_PRODUCCION" />
</appSettings>
```

---

---

# SECCIÓN DE PRODUCCIÓN — Despliegue del Servidor de Licencias

---

## P1 — Requisitos del servidor

| Componente | Mínimo recomendado |
|------------|-------------------|
| OS | Windows Server 2019/2022 (o Windows 10/11 Pro si es servidor interno) |
| RAM | 2 GB (la API es liviana) |
| Disco | 20 GB libres |
| .NET | .NET 10 Runtime (o Hosting Bundle si usas IIS) |
| SQL Server | SQL Server 2019+ Express / Standard / Developer |
| Red | Puerto 5000 (HTTP) o 5001 (HTTPS) abierto hacia los clientes |

---

## P2 — Preparar SQL Server en producción

### 2.1 Crear base de datos y usuario dedicado

Ejecuta en SQL Server Management Studio como `sa` o administrador:

```sql
-- Crear la base de datos
CREATE DATABASE SHEWebLicenseDb;
GO

-- Crear usuario dedicado para la app (NO usar sa en producción)
CREATE LOGIN she_app WITH PASSWORD = 'UnaContraseñaSegura123!';
GO

USE SHEWebLicenseDb;
GO

CREATE USER she_app FOR LOGIN she_app;
GO

-- Dar permisos mínimos necesarios
ALTER ROLE db_datareader ADD MEMBER she_app;
ALTER ROLE db_datawriter ADD MEMBER she_app;
-- Para que EF Core pueda crear/modificar tablas (migrations)
ALTER ROLE db_ddladmin ADD MEMBER she_app;
GO
```

### 2.2 Cadena de conexión resultante

```
Server=NOMBRE_SERVIDOR\INSTANCIA;Database=SHEWebLicenseDb;
User Id=she_app;Password=UnaContraseñaSegura123!;
TrustServerCertificate=True;Encrypt=True
```

> Si SQL Server está en la misma máquina que la API, usa `Server=localhost` o `Server=.`.

---

## P3 — Configurar `appsettings.Production.json`

El archivo ya existe en `SHE.License.API/appsettings.Production.json`.  
Edítalo con los valores reales **antes** de publicar:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SHEWebLicenseDb;User Id=she_app;Password=UnaContraseñaSegura123!;TrustServerCertificate=True;Encrypt=True"
  },
  "JwtSettings": {
    "Secret": "mín-32-chars-aleatorios-aqui-xK9#mP2$",
    "Issuer": "SHE.License.API",
    "Audience": "SHE.License.Web",
    "ExpirationMinutes": 480
  },
  "ApiKey": "clave-secreta-produccion-unica-aqui",
  "AllowedOrigins": "https://tu-dominio.com",
  "Kestrel": {
    "Endpoints": {
      "Http":  { "Url": "http://0.0.0.0:5000" },
      "Https": {
        "Url": "https://0.0.0.0:5001",
        "Certificate": {
          "Path": "C:\\Certificados\\she-api.pfx",
          "Password": "password-del-pfx"
        }
      }
    }
  }
}
```

> **Genera un secreto JWT seguro** con PowerShell:
> ```powershell
> [Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Max 256 }))
> ```

---

## P4 — Publicar la API

Desde la carpeta raíz del proyecto (`SHEWeb/`) ejecuta en PowerShell o CMD:

```powershell
dotnet publish SHE.License.API/SHE.License.API.csproj `
    --configuration Release `
    --output C:\Deploy\SHELicenseAPI `
    --runtime win-x64 `
    --self-contained false
```

Esto genera en `C:\Deploy\SHELicenseAPI`:
```
SHE.License.API.exe
SHE.License.API.dll
appsettings.json
appsettings.Production.json   ← este es el que se usará
web.config                    ← necesario si usas IIS
...
```

> **Importante:** nunca subas `appsettings.Development.json` al servidor. Solo
> `appsettings.json` (base) y `appsettings.Production.json` (valores reales).

---

## P5 — Opción A: Ejecutar como Servicio de Windows (recomendado para pyme)

Esta es la opción más simple. No requiere IIS.

### 5.1 Habilitar el soporte en el código

Agrega el paquete al proyecto de API:

```powershell
dotnet add SHE.License.API package Microsoft.Extensions.Hosting.WindowsServices
```

Agrega una sola línea en `SHE.License.API/Program.cs`, justo después de `var builder = WebApplication.CreateBuilder(args);`:

```csharp
builder.Host.UseWindowsService(); // ← agregar esta línea
```

Vuelve a publicar después de este cambio.

### 5.2 Instalar el servicio en el servidor

En el servidor, abre PowerShell como Administrador y ejecuta:

```powershell
# Crear el servicio
sc.exe create "SHELicenseAPI" `
    binPath= "C:\Deploy\SHELicenseAPI\SHE.License.API.exe" `
    start= auto `
    DisplayName= "SHEndevour License API"

# Configurar para reinicio automático en caso de fallo
sc.exe failure "SHELicenseAPI" reset= 86400 actions= restart/5000/restart/10000/restart/30000

# Establecer variable de entorno ASPNETCORE_ENVIRONMENT
sc.exe config "SHELicenseAPI" obj= "LocalSystem"

# Alternativa: usar un archivo de entorno
# Crea C:\Deploy\SHELicenseAPI\.env con: ASPNETCORE_ENVIRONMENT=Production
```

### 5.3 Configurar la variable de entorno del servicio

```powershell
# Setear ASPNETCORE_ENVIRONMENT=Production para que cargue appsettings.Production.json
[System.Environment]::SetEnvironmentVariable(
    "ASPNETCORE_ENVIRONMENT", "Production",
    [System.EnvironmentVariableTarget]::Machine)
```

### 5.4 Iniciar el servicio

```powershell
Start-Service "SHELicenseAPI"
Get-Service "SHELicenseAPI"   # debe mostrar Status: Running
```

### 5.5 Verificar que funciona

```powershell
# Desde el mismo servidor
Invoke-RestMethod -Uri "http://localhost:5000/api/health"
# Debe responder: @{Estado=OK; Timestamp=...}
```

---

## P6 — Opción B: Hostear en IIS

Usa esta opción si ya tienes IIS configurado en el servidor.

### 6.1 Instalar el .NET Hosting Bundle

Descarga e instala en el servidor:  
`https://dotnet.microsoft.com/download/dotnet/10.0` → **Hosting Bundle**

Reinicia IIS después de instalar:
```powershell
iisreset
```

### 6.2 Crear el sitio en IIS

1. Abre **IIS Manager** (`inetmgr`)
2. Click derecho en **Sites** → **Add Website**
3. Configura:
   - Site name: `SHELicenseAPI`
   - Physical path: `C:\Deploy\SHELicenseAPI`
   - Binding: HTTP, puerto `5000` (o HTTPS puerto `5001`)
4. En **Application Pools** → selecciona el pool del sitio → **Basic Settings**
   - `.NET CLR version`: **No Managed Code**
   - Pipeline mode: **Integrated**

### 6.3 Permisos de carpeta

```powershell
icacls "C:\Deploy\SHELicenseAPI" /grant "IIS AppPool\SHELicenseAPI:(OI)(CI)F"
icacls "C:\Deploy\SHELicenseAPI\logs" /grant "IIS AppPool\SHELicenseAPI:(OI)(CI)F"
```

### 6.4 Variable de entorno en IIS

En IIS Manager → Application Pool de `SHELicenseAPI` → **Advanced Settings**:
- En `Environment Variables` agrega: `ASPNETCORE_ENVIRONMENT = Production`

O directamente en `web.config` (ya incluido en el publish):
```xml
<aspNetCore processPath=".\SHE.License.API.exe" stdoutLogEnabled="false">
  <environmentVariables>
    <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  </environmentVariables>
</aspNetCore>
```

---

## P7 — Aplicar migraciones en producción

Una vez que la API está corriendo y apunta a la base de datos de producción, aplica las migraciones. Tienes dos opciones:

### Opción A — Desde la interfaz web (recomendado)

1. Abre el navegador y accede al panel web (`SHE.License.Web`) en producción
2. Al cargar, la pantalla de verificación detectará migraciones pendientes
3. Acepta aplicarlas en el diálogo

### Opción B — Desde la API con curl/PowerShell

```powershell
# Verificar migraciones pendientes
Invoke-RestMethod `
    -Uri "http://localhost:5000/api/admin/migrations/pending" `
    -Headers @{ "X-Api-Key" = "clave-secreta-produccion-unica-aqui" }

# Aplicar migraciones
Invoke-RestMethod `
    -Method POST `
    -Uri "http://localhost:5000/api/admin/migrations/apply" `
    -Headers @{ "X-Api-Key" = "clave-secreta-produccion-unica-aqui" }
```

### Opción C — Directo con EF CLI (en máquina de desarrollo apuntando a prod)

```powershell
# Desde la raíz del proyecto
dotnet ef database update `
    --project SHE.License.Infrastructure `
    --startup-project SHE.License.API `
    -- --connectionString "Server=SERVIDOR_PROD;Database=SHEWebLicenseDb;..."
```

---

## P8 — Configurar el Firewall de Windows

En el servidor, ejecuta como Administrador:

```powershell
# HTTP — para clientes que no usan HTTPS
New-NetFirewallRule -DisplayName "SHE License API HTTP" `
    -Direction Inbound -Protocol TCP -LocalPort 5000 -Action Allow

# HTTPS — recomendado para producción
New-NetFirewallRule -DisplayName "SHE License API HTTPS" `
    -Direction Inbound -Protocol TCP -LocalPort 5001 -Action Allow
```

> Si el servidor está detrás de un router, también abre el puerto en el router
> y configura reenvío (port forwarding) hacia la IP local del servidor.

---

## P9 — HTTPS con certificado (recomendado)

### Opción A — Certificado self-signed (para red interna)

Perfecto si los clientes solo se conectan desde la misma red:

```powershell
# Generar certificado self-signed válido 5 años
$cert = New-SelfSignedCertificate `
    -DnsName "she-license-api","localhost","TU_IP_SERVIDOR" `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -NotAfter (Get-Date).AddYears(5) `
    -KeyAlgorithm RSA -KeyLength 2048

# Exportar como PFX
$pwd = ConvertTo-SecureString -String "password-del-pfx" -Force -AsPlainText
Export-PfxCertificate `
    -Cert $cert `
    -FilePath "C:\Certificados\she-api.pfx" `
    -Password $pwd
```

Para que los clientes confíen en él, en cada PC cliente ejecuta:
```powershell
# Instalar el certificado en máquinas cliente (correr como admin)
Import-Certificate -FilePath "she-api.crt" `
    -CertStoreLocation "Cert:\LocalMachine\Root"
```

### Opción B — Let's Encrypt (si tienes dominio público)

1. Instala [win-acme](https://www.win-acme.com/) en el servidor
2. Ejecuta: `wacs.exe` y sigue el asistente para tu dominio
3. El certificado se renueva automáticamente cada 60 días

---

## P10 — Actualizar los clientes WPF para apuntar a producción

En el `App.config` de cada instalación de SHEndevour, cambia:

```xml
<appSettings>
  <!-- Cambiar localhost por la IP o dominio del servidor de producción -->
  <add key="LicenseApiUrl" value="https://192.168.1.100:5001" />
  <!-- O con dominio: -->
  <!-- <add key="LicenseApiUrl" value="https://licencias.tuempresa.com" /> -->
  <add key="LicenseApiKey" value="clave-secreta-produccion-unica-aqui" />
</appSettings>
```

> La `ApiKey` en el cliente debe coincidir exactamente con la del `appsettings.Production.json`.

---

## P11 — Lista de verificación de despliegue

Marca cada punto antes de dar acceso a los clientes:

```
[ ] SQL Server corriendo y base de datos SHEWebLicenseDb creada
[ ] Usuario she_app creado con permisos correctos
[ ] appsettings.Production.json configurado con valores reales (NO defaults)
[ ] API publicada en C:\Deploy\SHELicenseAPI
[ ] Servicio Windows creado e iniciado (o IIS configurado)
[ ] ASPNETCORE_ENVIRONMENT = Production aplicado
[ ] GET /api/health responde { Estado: "OK" } desde el servidor
[ ] GET /api/health responde desde una PC cliente (prueba de red)
[ ] Migraciones aplicadas (base de datos con todas las tablas)
[ ] Usuario admin creado y login funciona en el panel web
[ ] Puerto 5000/5001 abierto en firewall del servidor
[ ] Puerto abierto en router si los clientes se conectan desde fuera de la red
[ ] App.config de clientes WPF actualizado con la URL y ApiKey de producción
[ ] Prueba de activación de licencia exitosa desde un equipo cliente
[ ] Logs revisados en C:\Deploy\SHELicenseAPI\logs\ (sin errores críticos)
```

---

## Resumen de seguridad

| Amenaza | Protección |
|---------|------------|
| Clave inválida / inventada | La API rechaza con `Invalida_NoEncontrada` |
| Licencia de otro equipo | La API rechaza con `HardwareInvalido` |
| EmpresaId de otro cliente | Verificación local: `EmpresaId` almacenado ≠ recibido del servidor |
| Copiar storage a otra máquina | `CodigoInstalacion` local ≠ de la máquina → rechazada offline |
| API caída con licencia expirada | Validación offline bloquea si `FechaLicencia < hoy` |
| Acceso no autorizado a la API | `X-Api-Key` requerido en todos los endpoints de negocio |
| Contraseñas en texto plano | Passwords con BCrypt · Secretos en `appsettings.Production.json` |
| `EmpresaId` alterado manualmente | Es inmutable en storage y verificado contra la respuesta del servidor |

---

## Referencia de endpoints

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/health` | Ninguna | Prueba de conexión — usar para `TestConnectionAsync` |
| POST | `/api/licencia/obtener` | `X-Api-Key` | **Flujo principal.** Acepta `{EmpresaId, CodigoInstalacion}`, `{EmpresaId}` solo, o `{CodigoInstalacion, RFC}` (legacy). Devuelve `EmpresaId` + datos completos |
| POST | `/api/licencia/validar` | `X-Api-Key` | Valida `ClaveLicencia` + `CodigoInstalacion` criptográficamente. Devuelve `EmpresaId` en la respuesta |
| POST | `/api/auth/login` | Body: user/pass | Login del panel web — devuelve JWT |
| GET | `/api/admin/migrations/pending` | `X-Api-Key` | Lista migraciones pendientes |
| POST | `/api/admin/migrations/apply` | `X-Api-Key` | Aplica todas las migraciones pendientes |

---

---

# SECCIÓN UBUNTU SERVER — Despliegue con Docker + Nginx Proxy Manager + Cloudflare Tunnel

---

## Arquitectura de red

```
Internet
   ↓
Cloudflare DNS  (CNAME → TUNNEL_ID.cfargotunnel.com)
   ↓
Cloudflare Tunnel  (cloudflared en Docker, red proxy)
   ↓
Nginx Proxy Manager  (npm en Docker, red proxy, puerto 80/443)
   ↓  (enruta por hostname)
   ├── she-license-api:5000   (API de licencias)
   └── she-license-web:5287   (Panel web admin)
        ↓
   she-sqlserver:1433  (SQL Server, solo red interna)
```

Todo se comunica a través de la red Docker `proxy`. Los contenedores de la app solo usan `expose` (sin `ports`) — ningún puerto queda accesible desde fuera del servidor.

---

## Requisitos previos en el servidor

Antes de desplegar SHEWeb, el servidor debe tener:

| Componente | Estado requerido |
|------------|-----------------|
| Docker Engine | Instalado y activo |
| Docker Compose | Instalado (`docker compose version`) |
| Red `proxy` | Creada (`docker network create proxy`) |
| Nginx Proxy Manager | Corriendo en `/root/docker/` |
| cloudflared | Corriendo como contenedor en red `proxy` |

Si alguno no está listo, ver `GuiaUbuntuServer.md` para instalarlo.

---

## D1 — Requisitos previos en el servidor

Antes de clonar el proyecto, verifica que estas piezas estén operativas:

| Componente | Verificar |
|------------|-----------|
| Docker Engine | `docker --version` |
| Docker Compose v2 | `docker compose version` |
| Red `proxy` | `docker network ls \| grep proxy` |
| Nginx Proxy Manager | `docker ps \| grep npm` |
| cloudflared | `docker ps \| grep cloudflared` |

Si alguno no está listo, ver `GuiaUbuntuServer.md`.
Crear la red si no existe: `docker network create proxy`

---

## D2 — Estructura de directorios en el servidor

```
/root/docker/
├── docker-compose.yml          ← Nginx Proxy Manager (ya existente)
├── cloudflared/
│   ├── config.yml              ← Actualizar con rutas de SHEWeb
│   └── TU_TUNNEL_ID.json
└── apps/
    └── SHEWeb/                 ← El proyecto va aquí
        ├── docker-compose.yml
        ├── .env                ← Crear desde .env.example (NO en git)
        ├── SHE.License.API/
        └── SHE.License.Web/
```

---

## D3 — Clonar el repositorio

```bash
mkdir -p /root/docker/apps
cd /root/docker/apps
git clone https://github.com/TU_USUARIO/SHEWeb.git
cd SHEWeb

# Verificar estructura
ls -la
# Debe verse: docker-compose.yml  .env.example  .gitignore
```

> Si el repositorio es privado, agrega un deploy key SSH en GitHub → Settings → Deploy keys antes de clonar.

---

## D4 — Crear el archivo .env

El `.env` no está en git (está en `.gitignore`). Créalo manualmente:

```bash
cp .env.example .env
nano .env
# Guarda con Ctrl+O · Enter · Ctrl+X
```

Contenido con valores reales:

```bash
JWT_SECRET=genera-con-openssl-rand-base64-48
API_KEY=genera-con-openssl-rand-hex-32
ALLOWED_ORIGINS=https://panel.endevour.mx
```

Generar valores seguros:

```bash
openssl rand -base64 48   # JWT_SECRET
openssl rand -hex 32       # API_KEY
```

> **.env está en .gitignore** — nunca se sube al repositorio. Debe crearse en el servidor cada vez que se clona el proyecto.

---

## D5 — docker-compose.yml del proyecto

El `docker-compose.yml` en la raíz usa `expose` (no `ports`) y se integra a la red `proxy` existente:

```yaml
version: '3.9'

services:

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: she-sqlserver
    environment:
      SA_PASSWORD: "InfoHotel01"
      ACCEPT_EULA: "Y"
      MSSQL_PID: "Express"
    volumes:
      - sqldata:/var/opt/mssql
    expose:
      - "1433"
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \"InfoHotel01\" -Q \"SELECT 1\" -C -N -b -o /dev/null"]
      interval: 15s
      timeout: 10s
      retries: 10
      start_period: 30s
    networks:
      - proxy
    restart: unless-stopped

  she-api:
    build:
      context: .
      dockerfile: SHE.License.API/Dockerfile
    container_name: she-license-api
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:5000
      ConnectionStrings__DefaultConnection: "Server=she-sqlserver,1433;Database=SHEWebLicenseDb;User Id=sa;Password=InfoHotel01;TrustServerCertificate=True;Encrypt=True"
      JwtSettings__Secret: "${JWT_SECRET}"
      JwtSettings__Issuer: "SHE.License.API"
      JwtSettings__Audience: "SHE.License.Web"
      JwtSettings__ExpirationMinutes: "480"
      ApiKey: "${API_KEY}"
      AllowedOrigins: "${ALLOWED_ORIGINS}"
    expose:
      - "5000"
    networks:
      - proxy
    depends_on:
      sqlserver:
        condition: service_healthy
    restart: unless-stopped

  she-web:
    build:
      context: .
      dockerfile: SHE.License.Web/Dockerfile
    container_name: she-license-web
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:5287
      ConnectionStrings__DefaultConnection: "Server=she-sqlserver,1433;Database=SHEWebLicenseDb;User Id=sa;Password=InfoHotel01;TrustServerCertificate=True;Encrypt=True"
      JwtSettings__Secret: "${JWT_SECRET}"
      JwtSettings__Issuer: "SHE.License.API"
      JwtSettings__Audience: "SHE.License.Web"
      JwtSettings__ExpirationMinutes: "480"
    expose:
      - "5287"
    networks:
      - proxy
    depends_on:
      sqlserver:
        condition: service_healthy
    restart: unless-stopped

networks:
  proxy:
    external: true

volumes:
  sqldata:
```

> **`expose` vs `ports`**: `expose` solo abre el puerto dentro de la red Docker — NPM lo alcanza por nombre de contenedor. Ningún puerto queda expuesto a la IP del servidor.

---

## D6 — WebSockets en Program.cs

Tres cambios obligatorios en `SHE.License.Web/Program.cs` para que Blazor Server funcione en producción detrás de NPM y Cloudflare:

```csharp
// ① Circuit retention — evita perder el estado al reconectar
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
    });

// ② SignalR keepalive — Cloudflare cierra idle connections a 100 s
//    KeepAliveInterval = 15 s garantiza ping cada 15 s → nunca alcanza el límite
builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval     = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout          = TimeSpan.FromSeconds(15);
    options.KeepAliveInterval         = TimeSpan.FromSeconds(15);
    options.MaximumReceiveMessageSize = 32 * 1024;
});

var app = builder.Build();

// Producción: NPM y Cloudflare manejan TLS; UseHttpsRedirection causaría bucle
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

// ③ UseWebSockets ANTES de UseAuthentication y de MapRazorComponents
app.UseWebSockets();
app.UseAuthentication();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

| Cambio | Sin este cambio |
|--------|----------------|
| `DisconnectedCircuitRetentionPeriod = 10 min` | El circuit se destruye en segundos tras desconexión; al reconectar el usuario pierde el estado de la UI |
| `KeepAliveInterval = 15 s` | Cloudflare cierra la conexión WebSocket a los 100 s de inactividad |
| `app.UseWebSockets()` antes de `UseAuthentication` | El middleware de auth rechaza la petición WebSocket con 400 o 401 |
| `UseHttpsRedirection` solo en Development | En producción causa bucle infinito HTTP→HTTPS dentro del contenedor |

> Este código ya está aplicado en el repositorio. Revisa si clonaste antes de esta versión.

---

## D7 — Levantar los contenedores

```bash
cd /root/docker/apps/SHEWeb

# Primera vez — descarga imágenes base y compila (~3-5 min)
docker compose up -d --build

# Verificar que los tres contenedores están "running"
docker compose ps
# Esperado:
# she-sqlserver     running (healthy)
# she-license-api   running
# she-license-web   running

# Revisar logs si algo falla
docker compose logs --tail 30 she-license-web
# Buscar: "Now listening on http://[::]:5287"
```

> SQL Server tarda 30-60 s en ponerse en estado `healthy`. La API y el Web dependen de él (`condition: service_healthy`) — si no arrancan de inmediato, espera.

---

## D8 — Crear la base de datos

Esperar ~30 s a que SQL Server arranque, luego:

```bash
docker exec -it she-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "InfoHotel01" -C -N \
    -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name='SHEWebLicenseDb') CREATE DATABASE SHEWebLicenseDb;"
```

Aplicar migraciones de EF Core:

```bash
docker exec -it she-license-api \
    curl -s -X POST -H "X-Api-Key: TU_API_KEY" \
    http://localhost:5000/api/admin/migrations/apply
```

Verificar que la API responde:

```bash
docker exec -it she-license-api curl http://localhost:5000/api/health
# Esperado: {"estado":"OK","timestamp":"..."}
```

---

## D9 — Agregar rutas en cloudflared (config.yml)

Editar `/root/docker/cloudflared/config.yml`. **Todos los hostnames apuntan a `http://npm:80`** — NPM hace el enrutamiento por hostname. cloudflared pasa WebSockets transparentemente.

```yaml
tunnel: TU_TUNNEL_ID
credentials-file: /etc/cloudflared/TU_TUNNEL_ID.json

ingress:

  # Rutas ya existentes (conservar)
  - hostname: endevour.mx
    service: http://npm:80

  - hostname: grafana.endevour.mx
    service: http://npm:80

  # Rutas nuevas para SHEWeb
  - hostname: api.endevour.mx
    service: http://npm:80

  - hostname: panel.endevour.mx
    service: http://npm:80

  - service: http_status:404
```

Reiniciar cloudflared:

```bash
docker restart cloudflared
docker logs cloudflared --tail 20
# Buscar: "Registered tunnel connection"
```

---

## D10 — Crear registros DNS en Cloudflare

Dashboard Cloudflare → tu dominio → DNS → Add record:

| Tipo | Nombre | Contenido | Proxy |
|------|--------|-----------|-------|
| CNAME | `api` | `TU_TUNNEL_ID.cfargotunnel.com` | ON (naranja) |
| CNAME | `panel` | `TU_TUNNEL_ID.cfargotunnel.com` | ON (naranja) |

> El mismo `TUNNEL_ID` para todos los subdominios. SSL lo gestiona Cloudflare: **SSL/TLS → Overview → Full (strict)**.

---

## D11 — Nginx Proxy Manager — Proxy Hosts + WebSockets

Panel NPM: `http://IP_SERVIDOR:81` → **Proxy Hosts → Add Proxy Host**

### API de licencias

| Campo | Valor |
|-------|-------|
| Domain Names | `api.endevour.mx` |
| Forward Hostname / IP | `she-license-api` |
| Forward Port | `5000` |
| HTTP/2 Support | ✓ activado |
| SSL Certificate | Request a new SSL Certificate (Let's Encrypt) |
| Force SSL | ✓ activado |

### Panel web (Blazor Server) ★ WebSockets obligatorio

| Campo | Valor |
|-------|-------|
| Domain Names | `panel.endevour.mx` |
| Forward Hostname / IP | `she-license-web` |
| Forward Port | `5287` |
| HTTP/2 Support | ✓ activado |
| **WebSockets Support** | ✓ **ACTIVAR — obligatorio para Blazor Server** |
| SSL Certificate | Request a new SSL Certificate (Let's Encrypt) |
| Force SSL | ✓ activado |

> **WebSockets Support es el paso más crítico de todo el deploy.** Al activarlo NPM agrega `proxy_set_header Upgrade $http_upgrade` y `proxy_set_header Connection "upgrade"`. Sin estas cabeceras, el servidor rechaza el upgrade y Blazor muestra pantalla en blanco.

| Capa | WebSocket | Configuración manual |
|------|-----------|---------------------|
| Cloudflare Edge | ✓ Nativo | No |
| cloudflared | ✓ Transparente | No |
| Nginx Proxy Manager | ✓ Con checkbox | **Sí — WebSockets Support (D11)** |
| Kestrel / ASP.NET | ✓ Con middleware | **Sí — `app.UseWebSockets()` en Program.cs (D6)** |

---

## D12 — Verificar WebSockets

### Chrome DevTools (método más rápido)

1. Abrir `https://panel.endevour.mx` en Chrome.
2. Presionar `F12` → pestaña **Network**.
3. Hacer clic en el filtro **WS**.
4. Recargar la página (`Ctrl+Shift+R`).
5. Debe aparecer una entrada `_blazor` con status **101 Switching Protocols**.

> **101** = WebSocket establecido. Si ves **400** o no aparece ninguna entrada: revisa D6 (Program.cs) y D11 (NPM checkbox).

### curl desde el servidor

```bash
curl -i -N \
  -H "Host: panel.endevour.mx" \
  -H "Connection: Upgrade" \
  -H "Upgrade: websocket" \
  -H "Sec-WebSocket-Version: 13" \
  -H "Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==" \
  https://panel.endevour.mx/_blazor

# Respuesta esperada: HTTP/1.1 101 Switching Protocols
```

### Diagnóstico por código de error

| Respuesta | Causa | Solución |
|-----------|-------|----------|
| `101 Switching Protocols` | ✓ Todo correcto | — |
| `400 Bad Request` | `UseWebSockets()` no está o está en orden incorrecto en Program.cs | Revisar D6 |
| `200 OK` sin Upgrade | NPM no tiene WebSockets Support activado | Revisar D11 |
| `502 Bad Gateway` | Contenedor no corre o proxy mal configurado | Revisar `docker compose ps` y NPM |

### wscat (opcional, requiere Node.js)

```bash
npm install -g wscat
wscat -c wss://panel.endevour.mx/_blazor
# Resultado: conectado + mensajes SignalR en JSON
```

---

## D13 — Verificar el despliegue completo

```bash
# 1. Contenedores
docker compose ps

# 2. Health interno
docker exec -it she-license-api curl http://localhost:5000/api/health

# 3. Health desde internet
curl https://api.endevour.mx/api/health

# 4. Panel accesible
curl -I https://panel.endevour.mx

# 5. Logs si algo falla
docker compose logs -f she-license-api
docker logs cloudflared --tail 30
docker logs npm --tail 30
```

---

## D14 — GitHub Actions: deploy automático

Secretos en GitHub (Settings → Secrets → Actions):

| Secret | Valor |
|--------|-------|
| `SERVER_HOST` | IP del servidor Ubuntu |
| `SERVER_USER` | `root` |
| `SERVER_SSH_KEY` | Clave privada SSH completa |
| `SERVER_PROJECT_PATH` | `/root/docker/apps/SHEWeb` |

Generar clave SSH:

```bash
ssh-keygen -t ed25519 -C "github-deploy-sheweb" -f ~/.ssh/sheweb_deploy
ssh-copy-id -i ~/.ssh/sheweb_deploy.pub root@IP_SERVIDOR
# El contenido de ~/.ssh/sheweb_deploy va en SERVER_SSH_KEY
```

El workflow `.github/workflows/deploy.yml` ya está en el repositorio:

```yaml
name: Deploy to Ubuntu Server
on:
  push:
    branches: [ main ]
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Deploy via SSH
        uses: appleboy/ssh-action@v1.0.3
        with:
          host: ${{ secrets.SERVER_HOST }}
          username: ${{ secrets.SERVER_USER }}
          key: ${{ secrets.SERVER_SSH_KEY }}
          script: |
            cd ${{ secrets.SERVER_PROJECT_PATH }}
            git pull origin main
            docker compose up -d --build
            docker image prune -f
```

---

## D15 — Comandos de mantenimiento

```bash
# Actualizar (sin CI/CD)
cd /root/docker/apps/SHEWeb
git pull && docker compose up -d --build

# Reiniciar un servicio
docker restart she-license-api

# Logs en tiempo real
docker compose logs -f

# Uso de recursos
docker stats

# Backup de la base de datos
docker exec she-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "InfoHotel01" -C -N \
    -Q "BACKUP DATABASE SHEWebLicenseDb TO DISK='/var/opt/mssql/backup.bak'"

# Limpiar imágenes antiguas
docker image prune -f
```

---

## D16 — Errores comunes

| Error | Causa más probable | Solución |
|-------|-------------------|----------|
| Panel en blanco / pantalla vacía | WebSockets Support desactivado en NPM | NPM → editar proxy host `panel.endevour.mx` → Details → activar **WebSockets Support** |
| HTTP 400 en `/_blazor` | `UseWebSockets()` ausente o en orden incorrecto en Program.cs | Verificar D6: `app.UseWebSockets()` antes de `app.UseAuthentication()` |
| Desconexión frecuente (~100 s) | `KeepAliveInterval` muy alto — Cloudflare cierra idle connections a 100 s | Verificar D6: `KeepAliveInterval = TimeSpan.FromSeconds(15)` |
| Bucle de redirección (ERR_TOO_MANY_REDIRECTS) | `UseHttpsRedirection` activo en producción | Verificar D6: solo dentro de `if (IsDevelopment())` |
| `CF-1016` en Cloudflare | CNAME apunta a tunnel_id incorrecto | Verificar CNAME en DNS: `TU_TUNNEL_ID.cfargotunnel.com` |
| `502 Bad Gateway` en NPM | Forward Hostname/Port incorrectos o contenedor caído | Verificar nombre de contenedor y puerto. Revisar `docker compose ps` |
| NPM no llega al contenedor | Contenedor no está en red `proxy` | `docker network connect proxy she-license-web` |
| API no responde (primer inicio) | SQL Server aún arrancando | Esperar 30-60 s, revisar `docker compose ps` — esperar `healthy` |
| 404 en cloudflared | Hostname no está en `config.yml` | Agregar ruta en `/root/docker/cloudflared/config.yml` y reiniciar cloudflared |
| "Attempting to reconnect..." frecuente | Ver "Desconexión frecuente" arriba | Ver fila 3 |

---

## D17 — Checklist final (Ubuntu/Docker)

```
[ ] Red proxy existe: docker network ls | grep proxy
[ ] NPM corriendo: docker ps | grep npm
[ ] cloudflared corriendo: docker ps | grep cloudflared
[ ] Repositorio clonado en /root/docker/apps/SHEWeb
[ ] Archivo .env creado con JWT_SECRET, API_KEY y ALLOWED_ORIGINS

WebSockets en el código (D6):
[ ] Program.cs: app.UseWebSockets() ANTES de app.UseAuthentication()
[ ] Program.cs: UseHttpsRedirection solo dentro de if (IsDevelopment())
[ ] Program.cs: KeepAliveInterval = TimeSpan.FromSeconds(15) en AddSignalR

Contenedores:
[ ] docker compose up -d --build sin errores
[ ] she-sqlserver, she-license-api y she-license-web en estado running
[ ] she-sqlserver en estado healthy

Base de datos:
[ ] SHEWebLicenseDb creada
[ ] Migraciones de EF Core aplicadas
[ ] Health interno OK: docker exec she-license-api curl http://localhost:5000/api/health

Red y túnel:
[ ] config.yml de cloudflared actualizado con api.endevour.mx y panel.endevour.mx
[ ] cloudflared reiniciado y sin errores en logs
[ ] DNS CNAME creados en Cloudflare (Proxy ON)

NPM — WebSockets crítico:
[ ] Proxy host api.endevour.mx → she-license-api:5000 con SSL
[ ] Proxy host panel.endevour.mx → she-license-web:5287 con SSL + WebSockets Support activado

Verificación final:
[ ] Chrome DevTools: _blazor aparece con status 101
[ ] https://api.endevour.mx/api/health responde OK desde internet
[ ] Panel de login accesible en https://panel.endevour.mx (sin pantalla en blanco)
[ ] Login de usuario admin funcional
[ ] Navegar 2+ min sin desconexión involuntaria
[ ] GitHub Actions workflow configurado (4 secretos: HOST, USER, SSH_KEY, PATH)
[ ] App.config de clientes WPF actualizado con URL y ApiKey de producción
[ ] Prueba de activación de licencia exitosa desde equipo cliente
```

---

## D18 — Actualización del servidor

Cada vez que hagas `git push` a tu repositorio, sigue estos pasos en el servidor Ubuntu para aplicar los cambios.

### Actualización estándar

```bash
cd /root/docker/apps/SHEWeb

# 1. Obtener cambios del repositorio
git pull origin Master

# 2. Reconstruir imágenes y reiniciar contenedores
docker compose up -d --build

# 3. Limpiar imágenes antiguas
docker image prune -f
```

### Verificar después de actualizar

```bash
# Estado de los contenedores
docker compose ps

# Health check de la API
docker exec she-license-api curl -s http://localhost:5000/api/health
# Esperado: {"estado":"OK","timestamp":"..."}

# Logs recientes si algo falla
docker compose logs --tail 30 she-license-api
docker compose logs --tail 30 she-license-web
```

### Si el update incluye nuevas migraciones de EF Core

```bash
# Verificar migraciones pendientes
docker exec she-license-api curl -s \
  -H "X-Api-Key: TU_API_KEY" \
  http://localhost:5000/api/admin/migrations/pending

# Aplicar si devuelve una lista no vacía
docker exec she-license-api curl -s -X POST \
  -H "X-Api-Key: TU_API_KEY" \
  http://localhost:5000/api/admin/migrations/apply
```

### Rollback a versión anterior

```bash
# Ver últimos commits
git log --oneline -10

# Volver a un commit específico
git checkout <COMMIT_HASH>
docker compose up -d --build

# Volver al estado actual cuando esté listo
git checkout Master && git pull origin Master
docker compose up -d --build
```

### Script de actualización completa

Guarda esto en `/root/update_sheweb.sh` y úsalo como comando único:

```bash
#!/bin/bash
set -e
cd /root/docker/apps/SHEWeb
echo "[1/3] Obteniendo cambios..."
git pull origin Master
echo "[2/3] Reconstruyendo contenedores..."
docker compose up -d --build
echo "[3/3] Limpiando imágenes antiguas..."
docker image prune -f
echo "✓ Actualización completada"
docker compose ps
```

```bash
chmod +x /root/update_sheweb.sh
# Ejecutar con:
/root/update_sheweb.sh
```

---

## D19 — Backups y recuperación ante desastres

Estrategia mínima: backup diario de la BD + copia manual de secretos. Con esto puedes restaurar en un servidor nuevo en menos de 30 minutos.

### ¿Qué necesitas respaldar?

| Archivo / recurso | Ubicación | Crítico |
|---|---|---|
| Base de datos `SHEWebLicenseDb` | Volumen Docker `sqldata` | **Sí** — todos los datos |
| `.env` | `/root/docker/apps/SHEWeb/.env` | **Sí** — JWT_SECRET y API_KEY |
| Cloudflare tunnel credentials | `/root/docker/cloudflared/` | **Sí** — sin esto el túnel no funciona |
| NPM proxy hosts + SSL | `/root/docker/npm_data/` | Recomendado — evita reconfigurar NPM |
| Código fuente | GitHub | No — ya está en el repositorio |
| `docker-compose.yml` | GitHub | No — ya está en el repositorio |

### Script de backup

Crea `/root/backup_sheweb.sh`:

```bash
#!/bin/bash
set -e
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/root/backups/$DATE"
APP_DIR="/root/docker/apps/SHEWeb"

mkdir -p "$BACKUP_DIR"

echo "[1/4] Base de datos..."
docker exec she-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "InfoHotel01" -C -N \
    -Q "BACKUP DATABASE SHEWebLicenseDb TO DISK='/var/opt/mssql/bak_tmp.bak' WITH FORMAT, COMPRESSION"
docker cp she-sqlserver:/var/opt/mssql/bak_tmp.bak "$BACKUP_DIR/db.bak"
docker exec she-sqlserver rm /var/opt/mssql/bak_tmp.bak

echo "[2/4] Secretos y configuración..."
cp "$APP_DIR/.env" "$BACKUP_DIR/.env"
cp "$APP_DIR/docker-compose.yml" "$BACKUP_DIR/docker-compose.yml"

echo "[3/4] Cloudflare tunnel..."
cp -r /root/docker/cloudflared/ "$BACKUP_DIR/cloudflared/"

echo "[4/4] Nginx Proxy Manager..."
cp -r /root/docker/npm_data/ "$BACKUP_DIR/npm_data/" 2>/dev/null \
    || echo "  (npm_data no encontrado — omitido)"

tar -czf "/root/backups/sheweb_${DATE}.tar.gz" -C "/root/backups" "$DATE"
rm -rf "$BACKUP_DIR"
echo "✓ Backup: /root/backups/sheweb_${DATE}.tar.gz"
```

```bash
chmod +x /root/backup_sheweb.sh
mkdir -p /root/backups

# Ejecutar backup manual
/root/backup_sheweb.sh
```

### Backup diario automático (cron)

```bash
crontab -e

# Agrega esta línea — backup cada día a las 2:00 AM
0 2 * * * /root/backup_sheweb.sh >> /root/backups/backup.log 2>&1

# Verificar que quedó registrado
crontab -l
```

### Transferir backups fuera del servidor

> Un backup en el mismo servidor que sufre ransomware no sirve. Mueve los archivos a un segundo sitio.

```bash
# Opción A — SCP a otra máquina
scp /root/backups/sheweb_*.tar.gz usuario@OTRO_SERVIDOR:/backups/

# Opción B — rclone a Google Drive (recomendado)
curl https://rclone.org/install.sh | sudo bash
rclone config                 # sigue el asistente, elige Google Drive
rclone copy /root/backups/ gdrive:Backups/SHEWeb/ --include "*.tar.gz"

# Agregar rclone al cron (30 min después del backup)
30 2 * * * rclone copy /root/backups/ gdrive:Backups/SHEWeb/ --include "*.tar.gz" >> /root/backups/backup.log 2>&1

# Limpiar backups locales con más de 7 días
find /root/backups/ -name "sheweb_*.tar.gz" -mtime +7 -delete
```

### Procedimiento de restauración en servidor nuevo

```bash
# ── PREREQUISITOS ──────────────────────────────────────────────────────────
# Servidor nuevo con Docker + Docker Compose instalados (GuiaUbuntuServer.md)
docker network create proxy

# ── EXTRAER BACKUP ──────────────────────────────────────────────────────────
scp usuario@ORIGEN:/root/backups/sheweb_FECHA.tar.gz /tmp/
cd /tmp && tar -xzf sheweb_FECHA.tar.gz
BACKUP="/tmp/FECHA"       # ajusta al nombre de la carpeta extraída

# ── CLOUDFLARED ─────────────────────────────────────────────────────────────
mkdir -p /root/docker/cloudflared/
cp -r $BACKUP/cloudflared/* /root/docker/cloudflared/
# Levantar cloudflared con su docker-compose

# ── NPM ─────────────────────────────────────────────────────────────────────
mkdir -p /root/docker/npm_data/
cp -r $BACKUP/npm_data/* /root/docker/npm_data/
# Levantar NPM — proxy hosts y certificados ya estarán presentes

# ── APLICACIÓN ──────────────────────────────────────────────────────────────
git clone https://github.com/TU_USUARIO/SHEWeb.git /root/docker/apps/SHEWeb
cp "$BACKUP/.env" /root/docker/apps/SHEWeb/.env

# ── CONTENEDORES ────────────────────────────────────────────────────────────
cd /root/docker/apps/SHEWeb
docker compose up -d
# Esperar ~60 s a que SQL Server esté healthy
watch docker compose ps    # Ctrl+C cuando los tres estén Running

# ── RESTAURAR BD ────────────────────────────────────────────────────────────
docker cp "$BACKUP/db.bak" she-sqlserver:/var/opt/mssql/restore.bak

docker exec she-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "InfoHotel01" -C -N \
    -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name='SHEWebLicenseDb') \
        CREATE DATABASE SHEWebLicenseDb;"

docker exec she-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "InfoHotel01" -C -N \
    -Q "RESTORE DATABASE SHEWebLicenseDb FROM DISK='/var/opt/mssql/restore.bak' \
        WITH REPLACE, RECOVERY;"

docker exec she-sqlserver rm /var/opt/mssql/restore.bak

# ── VERIFICAR ────────────────────────────────────────────────────────────────
docker exec she-license-api curl http://localhost:5000/api/health
```

### Checklist de restauración

```
[ ] Servidor nuevo con Docker + Compose instalados
[ ] Red proxy creada: docker network create proxy
[ ] Backup descomprimido en /tmp/
[ ] cloudflared restaurado y corriendo (túnel conectado en Cloudflare dashboard)
[ ] NPM restaurado y levantado (proxy hosts y certificados presentes)
[ ] Repositorio clonado en /root/docker/apps/SHEWeb
[ ] .env restaurado con JWT_SECRET y API_KEY originales
[ ] docker compose up -d sin errores
[ ] she-sqlserver en estado healthy
[ ] Base de datos restaurada desde db.bak
[ ] Health check OK: docker exec she-license-api curl http://localhost:5000/api/health
[ ] Panel accesible desde internet: https://panel.endevour.mx
[ ] Login admin funcional
[ ] Prueba de licencia desde equipo WPF exitosa
```
