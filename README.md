# Clientes API (.NET 8) — A1

API REST en .NET 8 con endpoint protegido por OAuth2/JWT (Keycloak), documentación Swagger/OpenAPI, logging con Serilog y manejo centralizado de excepciones.

---

## Requisitos
- .NET SDK 8
- Keycloak (local) + Java (JDK 21 recomendado)

---

## Estructura del proyecto
Solución en capas:
- ClientesAPI: API (Controllers, Middleware, Swagger, Auth, DI)
- Clientes.CORE: Modelos/DTOs + interfaces (contratos)
- Clientes.BLL: Servicios de negocio
- Clientes.DAL: Repositorios (mock/in-memory)

---

## Endpoint
GET /api/clientes/{id}

Comportamiento:
- 200 si el cliente existe
- 404 si no existe
- 401 si no hay token o es inválido

---

## Logging
- Serilog escribe en: ./Logs/log.txt
- Incluye TraceId por request

---

## Manejo de errores
- Middleware global captura excepciones no controladas
- Responde application/problem+json con traceId
- Loguea el error en Logs/log.txt

---

## Seguridad (Keycloak + JWT)

### 1) Instalar/Preparar Keycloak (local)
1. Descargar Keycloak (zip) desde la página oficial.
2. Descomprimir, por ejemplo en: C:\tools\keycloak-26.4.7
3. Tener instalado Java (JDK 21 recomendado).
   Nota: si aparece el warning "JAVA_HOME is not set", puede funcionar igual, pero se recomienda configurarlo.

### 2) Levantar Keycloak (PowerShell)
En PowerShell, dentro de la carpeta de Keycloak:

$env:KC_BOOTSTRAP_ADMIN_USERNAME="admin"
$env:KC_BOOTSTRAP_ADMIN_PASSWORD="admin"
.\bin\kc.bat start-dev

Debe quedar escuchando en: http://localhost:8080

### 3) Configuración necesaria en Keycloak (resumen)
En el realm clientes-realm:
- Crear un Client llamado: clientes-api
- Crear un User llamado: tester con password: Tester123!
- Habilitar el usuario (Enabled)

### 4) Pedir token (PowerShell)

JSON completo:
curl.exe -X POST "http://localhost:8080/realms/clientes-realm/protocol/openid-connect/token" `
  -H "Content-Type: application/x-www-form-urlencoded" `
  -d "grant_type=password" `
  -d "client_id=clientes-api" `
  -d "username=tester" `
  -d "password=Tester123!" `
  -d "scope=openid"

Solo access_token (opcional):
($(
  curl.exe -s -X POST "http://localhost:8080/realms/clientes-realm/protocol/openid-connect/token" `
    -H "Content-Type: application/x-www-form-urlencoded" `
    -d "grant_type=password" `
    -d "client_id=clientes-api" `
    -d "username=tester" `
    -d "password=Tester123!" `
    -d "scope=openid"
) | ConvertFrom-Json).access_token

### 5) Autorizar en Swagger
1. Abrir Swagger: https://localhost:{puerto}/swagger
2. Click Authorize
3. Pegar solo el token (no escribir "Bearer"; Swagger lo agrega)
4. Click Authorize y probar el endpoint

---

## Configuración (appsettings.json)
- Auth:Authority: http://localhost:8080/realms/clientes-realm

---

## Cómo ejecutar la API
dotnet run --project ClientesAPI

---

## Decisiones de diseño
- Separación por capas (CORE/BLL/DAL/API) para mantener contratos y responsabilidades claras.
- Repositorio mock/in-memory para A1 (permite probar el endpoint sin BD).
- Auth con Keycloak (OAuth2/JWT) para proteger el endpoint.
- Serilog para logs a archivo y trazabilidad por request.
- Middleware global para manejo consistente de errores.


---

## A2 — SQL ( SQL Server)

Para obtener los **5 clientes con mayor saldo total** (sumando el saldo de todas sus cuentas), usaría un `JOIN` entre Clientes y Cuentas y agruparía por cliente.

Asumo tablas:
- `Clientes(id, nombre)`
- `Cuentas(id, cliente_id, saldo)`

SQL:
SELECT TOP (5)
    c.id,
    c.nombre,
    SUM(COALESCE(a.saldo, 0)) AS saldo_total
FROM dbo.Clientes c
LEFT JOIN dbo.Cuentas a ON a.cliente_id = c.id
GROUP BY c.id, c.nombre
ORDER BY saldo_total DESC;

Nota: dejé `LEFT JOIN` para que aparezcan clientes que no tienen cuentas (saldo = 0).  


### Mejora de rendimiento (SQL Server / Azure SQL)
En una base con hartos datos, lo primero que agregaría es un índice en `Cuentas(cliente_id)`, incluyendo `saldo` para ayudar al join y al cálculo:

SQL:
CREATE NONCLUSTERED INDEX IX_Cuentas_ClienteId
ON dbo.Cuentas (cliente_id)
INCLUDE (saldo);

(Con eso normalmente mejora el plan del join y baja lecturas.)

---

## A3 — Despliegue en Azure, CI/CD e IaC

### 1) Publicación en Azure App Service
1. Crear un **Resource Group** (ej: `rg-clientes-dev`).
2. Crear un **App Service Plan** .
3. Crear la **Web App** (ej: `clientes-api-dev`) con runtime **.NET 8**.
4. Configurar **Application Settings** en la Web App (variables de entorno), por ejemplo:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `Auth__Authority=<URL Keycloak Realm>`
   - cualquier otra config necesaria.
5. Activar **HTTPS Only** y, si aplica, restricciones de acceso (por IP o redes).

### 2) CI/CD con Azure DevOps
pipeline sencillo con dos etapas:

**Build**
- Restore / Build / Test:
  - `dotnet restore`
  - `dotnet build -c Release`
  - `dotnet test -c Release`
- Publicar artefacto:
  - `dotnet publish -c Release -o <output>` y empaquetar en zip (artifact).

**Deploy**
- Tomar el artefacto y desplegar a App Service con la tarea de Azure DevOps (zip deploy).
- Idealmente dejarlo gatillado con merge a `main` o manual con aprobación.

### 3) Manejo seguro de secretos
- No dejar secretos en el repo ni en el YAML.
- Guardar secretos en **Azure Key Vault** (connection strings, client secrets si existieran, etc.).
- En la Web App activaría **Managed Identity** y le daría permisos mínimos al Key Vault.
- Consumir secretos desde Key Vault o inyectarlos como App Settings (según el caso).

### 4) Escalabilidad
- **Scale up** (más CPU/memoria) si el consumo es alto.
- **Scale out** (más instancias) con **Autoscale** basado en métricas (CPU, requests, etc.).
- Mantener la API stateless para que escale sin problemas (sin depender de sesión en memoria).

### 5) Monitoreo y logs
- Habilitar **Application Insights** para ver requests, errores, dependencias y performance.
- Configurar **Diagnostic settings** del App Service para enviar logs a Log Analytics si se requiere centralización.
- Mantener trazabilidad con un identificador por request (en el proyecto ya se usa `TraceId`).

### 6) Infraestructura como código (IaC)
Usaría **Bicep** (o Terraform/ARM) para que el entorno se pueda recrear sin pasos manuales:
- Resource Group
- App Service Plan
- Web App + App Settings
- Application Insights
- Key Vault + permisos de Managed Identity

En el pipeline dejaría un stage opcional “Infra” para aplicar el template (IaC) y luego el stage de despliegue de la API.

