# Deploy Completo ASP.NET + Docker + Cloudflare Tunnel + Nginx Proxy Manager en Ubuntu Server

---

# 🚀 Arquitectura Final Funcional

```text
Internet
   ↓
Cloudflare DNS
   ↓
Cloudflare Tunnel
   ↓
Nginx Proxy Manager
   ↓
Contenedor Docker ASP.NET
```

---

# ✅ Objetivo

Esta guía permite desplegar:

* APIs ASP.NET
* Blazor Hybrid / Blazor Web
* ASP.NET Minimal API
* ASP.NET MVC
* Sitios HTML estáticos
* Microservicios Docker

usando:

* Ubuntu Server
* Docker
* Docker Compose
* Cloudflare Tunnel
* Nginx Proxy Manager

SIN abrir puertos del router.

---

# 🔥 Estructura Recomendada del Servidor

```text
/root/docker
│
├── docker-compose.yml          # Nginx Proxy Manager
│
├── cloudflared/
│   ├── cert.pem
│   ├── config.yml
│   └── TUNNEL_ID.json
│
├── monitoring/
│   ├── docker-compose.yml
│   └── ...
│
├── landing/
│   ├── index.html
│   ├── css/
│   ├── js/
│   └── assets/
│
├── apps/
│   ├── mi-api/
│   ├── mi-blazor/
│   └── otra-api/
```

---

# 🔥 1. Instalar Docker

```bash
apt update && apt upgrade -y

curl -fsSL https://get.docker.com | sh

systemctl enable docker
systemctl start docker
```

Verificar:

```bash
docker --version
```

---

# 🔥 2. Instalar Docker Compose

```bash
apt install docker-compose-plugin -y
```

Verificar:

```bash
docker compose version
```

---

# 🔥 3. Crear Red Global Proxy

MUY IMPORTANTE.

Esta red conecta:

* Cloudflare
* Nginx
* APIs
* Grafana
* Landing pages

```bash
docker network create proxy
```

Verificar:

```bash
docker network ls
```

---

# 🔥 4. Instalar Nginx Proxy Manager

## Crear carpeta

```bash
mkdir -p /root/docker/npm
cd /root/docker
```

---

## docker-compose.yml

```yaml
version: "3.9"

services:

  npm:
    image: jc21/nginx-proxy-manager:latest
    container_name: npm
    restart: unless-stopped

    ports:
      - "80:80"
      - "81:81"
      - "443:443"

    volumes:
      - ./npm/data:/data
      - ./npm/letsencrypt:/etc/letsencrypt

    networks:
      - proxy

networks:
  proxy:
    external: true
```

---

## Levantar

```bash
docker compose up -d
```

---

# 🔥 5. Acceder a Nginx Proxy Manager

```text
http://IP_DEL_SERVIDOR:81
```

Login:

```text
Email:
admin@example.com

Password:
changeme
```

Cambiar credenciales.

---

# 🔥 6. Instalar Cloudflare Tunnel

---

# Crear carpeta

```bash
mkdir -p /root/docker/cloudflared
```

---

# Login Cloudflare

```bash
docker run -it --rm \
  -v /root/docker/cloudflared:/etc/cloudflared \
  cloudflare/cloudflared:latest \
  tunnel login
```

Abrir URL.

Seleccionar dominio.

---

# Crear tunnel

```bash
docker run -it --rm \
  -v /root/docker/cloudflared:/etc/cloudflared \
  cloudflare/cloudflared:latest \
  tunnel create production
```

---

# 🔥 Configuración config.yml

```yaml
tunnel: TU_TUNNEL_ID
credentials-file: /etc/cloudflared/TU_TUNNEL_ID.json

ingress:

  - hostname: endevour.mx
    service: http://npm:80

  - hostname: api.endevour.mx
    service: http://npm:80

  - hostname: grafana.endevour.mx
    service: http://npm:80

  - service: http_status:404
```

---

# 🔥 Levantar Cloudflared

```bash
docker run -d \
  --name cloudflared \
  --restart unless-stopped \
  --network proxy \
  -v /root/docker/cloudflared:/etc/cloudflared \
  cloudflare/cloudflared:latest \
  tunnel --config /etc/cloudflared/config.yml run
```

---

# 🔥 7. Configurar DNS Cloudflare

## Ir a:

```text
Cloudflare
→ DNS
```

---

# Crear registros

## Ejemplo API

```text
api.endevour.mx
```

Tipo:

```text
CNAME
```

Contenido:

```text
TU_TUNNEL_ID.cfargotunnel.com
```

Proxy:

```text
ON
```

---

# Ejemplo Landing

```text
endevour.mx
```

→ mismo CNAME.

---

# 🔥 8. Conectar Contenedores a la Red Proxy

MUY IMPORTANTE.

Todos los servicios web deben estar en:

```text
proxy
```

---

# Ejemplo:

```bash
docker network connect proxy mi-api
```

---

# 🔥 9. Deploy API ASP.NET

---

# Estructura

```text
/root/docker/apps/mi-api
│
├── Dockerfile
├── docker-compose.yml
└── codigo...
```

---

# Dockerfile ASP.NET

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MiApi.dll"]
```

---

# docker-compose.yml

```yaml
version: "3.9"

services:

  mi-api:
    build: .
    container_name: mi-api
    restart: unless-stopped

    expose:
      - "8080"

    networks:
      - proxy

networks:
  proxy:
    external: true
```

---

# Build y Deploy

```bash
cd /root/docker/apps/mi-api

docker compose up -d --build
```

---

# 🔥 10. Configurar Nginx Proxy Manager

## Proxy Hosts

### Domain

```text
api.endevour.mx
```

---

### Forward Hostname

```text
mi-api
```

---

### Port

```text
8080
```

---

### SSL

Activar:

* SSL Certificate
* Force SSL

---

# 🚀 LISTO

Ahora:

```text
https://api.endevour.mx
```

funciona públicamente.

---

# 🔥 11. Deploy Landing HTML

---

# Crear carpeta

```bash
mkdir -p /root/docker/landing
```

---

# Meter:

```text
index.html
css/
js/
assets/
```

---

# Levantar nginx estático

```bash
docker run -d \
  --name landing \
  --restart unless-stopped \
  --network proxy \
  -v /root/docker/landing:/usr/share/nginx/html:ro \
  nginx:alpine
```

---

# Nginx Proxy Manager

## Domain

```text
endevour.mx
```

---

## Forward Hostname

```text
landing
```

---

## Port

```text
80
```

---

# 🚀 LISTO

---

# 🔥 12. Deploy desde GitHub

---

# Clonar repo

```bash
cd /root/docker/apps

git clone TU_REPO
```

---

# Actualizar

```bash
cd repo

git pull
```

---

# Rebuild

```bash
docker compose up -d --build
```

---

# 🔥 13. Logs y Diagnóstico

---

# Logs API

```bash
docker logs mi-api
```

---

# Logs Cloudflare

```bash
docker logs cloudflared
```

---

# Logs Nginx

```bash
docker logs npm
```

---

# Ver redes

```bash
docker network inspect proxy
```

---

# Ver contenedores

```bash
docker ps
```

---

# 🔥 14. Backup del Servidor

---

# Backup Docker

```bash
tar -czvf backup.tar.gz /root/docker
```

---

# Restaurar

```bash
tar -xzvf backup.tar.gz -C /
```

---

# 🔥 15. Actualizar Contenedores

```bash
docker compose pull
docker compose up -d
```

---

# 🔥 16. Reinicio Completo

```bash
docker restart npm
docker restart cloudflared
docker restart mi-api
```

---

# 🔥 17. Flujo Correcto de Deploy

```text
1. git pull
2. docker compose up -d --build
3. verificar docker ps
4. verificar logs
5. probar dominio
```

---

# 🔥 18. Errores Comunes

---

## ❌ Error 404

Cloudflare no encuentra ruta.

Verificar:

```yaml
ingress:
```

---

## ❌ Error 1016

DNS incorrecto.

Verificar:

```text
CNAME → TUNNEL_ID.cfargotunnel.com
```

---

## ❌ Nginx no encuentra contenedor

Contenedor NO está en:

```text
proxy
```

---

## ❌ No carga dominio

Verificar:

```bash
docker exec -it npm curl http://contenedor
```

---

# 🚀 Resultado Final

Infraestructura profesional:

✅ Cloudflare Tunnel
✅ SSL automático
✅ Docker
✅ ASP.NET
✅ Nginx Proxy Manager
✅ Subdominios
✅ APIs
✅ Landing Pages
✅ Grafana
✅ Prometheus
✅ Escalable
✅ Sin abrir puertos
✅ Producción real
