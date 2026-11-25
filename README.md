# E-Commerce Microservicios — Catálogo, Identity y Pedidos

Plataforma de microservicios para un e-commerce de productos electrónicos, implementada en **.NET 10** y **PostgreSQL 17** con **Docker Compose**. Aplica **Clean Architecture**, **Domain-Driven Design (DDD)**, **CQRS con MediatR**, autenticación centralizada con **ASP.NET Core Identity + JWT (HS256)**, autorización basada en **Roles** (`Admin`, `Vendedor`, `Cliente`) y propagación de token entre microservicios (`Pedidos` → `Catalog`).

## Integrantes

- Simón Vanegas
- Efrén Cuadrado
- Sofía Castrillón
- Franyelica García
- Santiago Castaño

---

## Requisitos

- [Docker](https://www.docker.com/) y [Docker Compose](https://docs.docker.com/compose/)
- *(Opcional)* [.NET 10 SDK](https://dotnet.microsoft.com/) para desarrollo local sin contenedores.

---

## Instrucciones para Ejecutar el Proyecto

### 1. Configurar el archivo `.env` (Obligatorio)

Los tres microservicios comparten la clave simétrica `JWT_KEY` desde un archivo `.env` en la raíz del proyecto. **Sin este archivo `docker compose` no arranca.**

Puedes copiar el archivo de ejemplo incluido en el repositorio:

```bash
cp .env.example .env
```

O crear manualmente el archivo `.env` en la raíz con el siguiente contenido:

```env
JWT_KEY=i9Tkp83ubKEMy6feh/vgLUxAC+ICbcwTPN5PHzUIGKvqrpCnEfKUhJRxWED3/6N5
```

### 2. Levantar los 7 contenedores con Docker Compose

```bash
docker compose up -d --build
```

El entorno levanta el frontend web, 3 microservicios y 3 bases de datos PostgreSQL independientes definidas en [`docker-compose.yml`](docker-compose.yml):

| Servicio | Descripción | Puerto Host | Swagger UI |
| :--- | :--- | :--- | :--- |
| `web` | **Frontend web** (ASP.NET Core MVC). Llama a los 3 microservicios desde el servidor con el JWT del usuario | `5000` | **http://localhost:5000** (interfaz) |
| `identity-api` | Microservicio de autenticación, usuarios, roles y emisión de JWT | `5081` | http://localhost:5081/swagger |
| `identity-db` | PostgreSQL 17 de Identity (`identity`) | `5434` | — |
| `catalog-api` | Microservicio de catálogo de productos y categorías (CQRS/DDD) | `5080` | http://localhost:5080/swagger |
| `catalog-db` | PostgreSQL 17 de Catálogo (`catalog`) | `5433` | — |
| `pedidos-api` | Microservicio de pedidos (consulta `catalog-api` reenviando el JWT) | `5082` | http://localhost:5082/swagger |
| `orders-db` | PostgreSQL 17 de Pedidos (`orders`) | `5435` | — |

Cada API espera a que su base de datos esté saludable (`healthcheck`) y aplica automáticamente sus migraciones de Entity Framework Core al iniciar en `Development` (además, `identity-api` ejecuta los Seeders de usuarios y roles).

### 3. Comandos útiles de Docker Compose

```bash
# Ver el estado de los 6 contenedores
docker compose ps

# Ver logs en tiempo real de todos los servicios (o de uno en específico)
docker compose logs -f
docker compose logs -f identity-api catalog-api pedidos-api

# Detener y eliminar contenedores y volúmenes (para reiniciar con bases limpias)
docker compose down -v
```

---

## Usuarios Iniciales (Seeders de `Identity.API`)

Al iniciar, `Identity.API` aplica las migraciones y ejecuta `IdentitySeeder` de forma idempotente:

| Correo | Rol | Contraseña (desarrollo) | Permisos principales |
| :--- | :--- | :--- | :--- |
| `admin@ecommerce.local` | `Admin` | `Admin123!` | Crear/desactivar categorías y productos, consultar catálogo y todos los pedidos |
| `vendedor@ecommerce.local` | `Vendedor` | `Vendedor123!` | Crear productos, actualizar precio/stock, consultar y cambiar estado de pedidos |
| `cliente@ecommerce.local` | `Cliente` | `Cliente123!` | Consultar catálogo, crear pedidos y consultar únicamente sus propios pedidos |

Para verificar en la base de datos que los usuarios y roles fueron creados por el Seeder:

```bash
docker compose exec identity-db psql -U identity -d identity -c \
  'SELECT u."Email", r."Name" AS rol FROM "AspNetUsers" u
   JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id"
   JOIN "AspNetRoles" r ON r."Id" = ur."RoleId" ORDER BY r."Name";'
```

---

## Matriz de Acceso por Roles y Endpoints

### 1. Identity (`http://localhost:5081`)

| Método | Ruta | Acceso | Descripción | Respuestas |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Público | Autentica con correo y contraseña y devuelve el JWT (`accessToken`) | `200 OK`, `401 Unauthorized` |

### 2. Catálogo (`http://localhost:5080`)

| Método | Ruta | Roles permitidos | Descripción | Respuestas |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/categories` | `Admin`, `Vendedor`, `Cliente` | Lista todas las categorías | `200 OK`, `401` |
| `GET` | `/api/categories/{id}` | `Admin`, `Vendedor`, `Cliente` | Obtiene una categoría por ID | `200 OK`, `401`, `404` |
| `POST` | `/api/categories` | `Admin` | Crea una nueva categoría | `201 Created`, `400`, `401`, `403` |
| `GET` | `/api/products` | `Admin`, `Vendedor`, `Cliente` | Lista productos (`?categoryId={guid}` opcional) | `200 OK`, `401` |
| `GET` | `/api/products/{id}` | `Admin`, `Vendedor`, `Cliente` | Obtiene un producto por ID | `200 OK`, `401`, `404` |
| `POST` | `/api/products` | `Admin`, `Vendedor` | Crea un nuevo producto | `201 Created`, `400`, `401`, `403` |
| `PATCH` | `/api/products/{id}/price` | `Admin`, `Vendedor` | Actualiza el precio y moneda del producto | `204 No Content`, `400`, `401`, `403`, `404` |
| `PATCH` | `/api/products/{id}/stock` | `Admin`, `Vendedor` | Ajusta el inventario (suma o resta) | `204 No Content`, `400`, `401`, `403`, `404` |
| `PATCH` | `/api/products/{id}/deactivate` | `Admin` | Desactiva un producto | `204 No Content`, `400`, `401`, `403`, `404` |

### 3. Pedidos (`http://localhost:5082`)

| Método | Ruta | Roles permitidos | Descripción | Respuestas |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/orders` | `Cliente` | Crea un pedido (reenvía el JWT a `Catalog` para validar precio y producto) | `201 Created`, `400`, `401`, `403` |
| `GET` | `/api/orders` | `Admin`, `Vendedor`, `Cliente` | Lista pedidos (`Cliente` ve solo los suyos; `Admin`/`Vendedor` ven todos) | `200 OK`, `401` |
| `GET` | `/api/orders/{id}` | `Admin`, `Vendedor`, `Cliente` | Obtiene un pedido por ID (`403` si un `Cliente` consulta el de otro usuario) | `200 OK`, `401`, `403`, `404` |
| `PATCH` | `/api/orders/{id}/status` | `Vendedor` | Cambia el estado del pedido (`Confirmed`, `Shipped`, `Delivered`, `Cancelled`) | `200 OK`, `400`, `401`, `403`, `404` |

---

## Guía Paso a Paso de Pruebas en Swagger UI

Tanto **Catalog API** (`http://localhost:5080/swagger`) como **Pedidos API** (`http://localhost:5082/swagger`) cuentan con el botón **Authorize** configurado con esquema HTTP Bearer JWT: solo debes pegar el valor de `accessToken` (sin escribir la palabra `Bearer`).

### Paso 1: Probar el acceso sin token (`401 Unauthorized`)

1. Abrir **Catalog Swagger**: http://localhost:5080/swagger (asegurarse de no tener sesión activa en el botón **Authorize**).
2. Desplegar `GET /api/products` → clic en **Try it out** → **Execute**.
   - **Resultado esperado:** `401 Unauthorized` (cabecera `www-authenticate: Bearer`).
3. Abrir **Pedidos Swagger**: http://localhost:5082/swagger.
4. Desplegar `GET /api/orders` o `POST /api/orders` → **Try it out** → **Execute**.
   - **Resultado esperado:** `401 Unauthorized`.

---

### Paso 2: Obtener tokens válidos en Identity (`200 OK`)

1. Abrir **Identity Swagger**: http://localhost:5081/swagger.
2. Desplegar `POST /api/auth/login` → **Try it out**.
3. Ejecutar con cada uno de los 3 usuarios y copiar el campo `accessToken` de la respuesta `200 OK`:
   - **Cliente:**
     ```json
     {
       "email": "cliente@ecommerce.local",
       "password": "Cliente123!"
     }
     ```
   - **Vendedor:**
     ```json
     {
       "email": "vendedor@ecommerce.local",
       "password": "Vendedor123!"
     }
     ```
   - **Admin:**
     ```json
     {
       "email": "admin@ecommerce.local",
       "password": "Admin123!"
     }
     ```

---

### Paso 3: Probar el acceso con un token válido (`200 OK`)

1. En **Catalog Swagger** (http://localhost:5080/swagger), hacer clic en el botón **Authorize** (arriba a la derecha).
2. Pegar el `accessToken` del **Cliente** en el campo `Value` → clic en **Authorize** → **Close**.
3. Ejecutar `GET /api/products` y `GET /api/categories`.
   - **Resultado esperado:** `200 OK`.

---

### Paso 4: Verificar el acceso según los roles (`403 Forbidden` vs `201 Created` / `200 OK`)

#### A. En Catalog Swagger (http://localhost:5080/swagger)

1. **Con token de `Cliente`** (ya autenticado en el paso anterior):
   - Ejecutar `POST /api/categories` con:
     ```json
     {
       "name": "Portátiles",
       "description": "Equipos portátiles"
     }
     ```
     - **Resultado esperado:** `403 Forbidden` (solo `Admin` puede crear categorías).
   - Ejecutar `POST /api/products`.
     - **Resultado esperado:** `403 Forbidden` (solo `Admin` o `Vendedor` pueden crear productos).

2. **Cambiar al token de `Vendedor`** (botón **Authorize** → **Logout** → pegar `accessToken` de `Vendedor` → **Authorize**):
   - Ejecutar `POST /api/categories` con `{"name": "Portátiles", "description": "Equipos portátiles"}`.
     - **Resultado esperado:** `403 Forbidden` (solo `Admin` puede crear categorías).

3. **Cambiar al token de `Admin`** (botón **Authorize** → **Logout** → pegar `accessToken` de `Admin` → **Authorize**):
   - Ejecutar `POST /api/categories` con:
     ```json
     {
       "name": "Portátiles",
       "description": "Equipos portátiles"
     }
     ```
     - **Resultado esperado:** `201 Created`. Copiar el `id` de la categoría creada.

4. **Volver al token de `Vendedor`** (botón **Authorize** → **Logout** → pegar `accessToken` de `Vendedor` → **Authorize**):
   - Ejecutar `POST /api/products` reemplazando `categoryId` por el `id` obtenido:
     ```json
     {
       "name": "Lenovo ThinkPad X1 Carbon",
       "description": "Portátil 14 pulgadas, 16 GB RAM, 512 GB SSD",
       "price": 7500000,
       "currency": "COP",
       "stock": 10,
       "categoryId": "PEGAR_AQUI_CATEGORY_ID"
     }
     ```
     - **Resultado esperado:** `201 Created`. Copiar el `id` del producto creado.
   - Ejecutar `PATCH /api/products/{id}/deactivate` con el `id` del producto.
     - **Resultado esperado:** `403 Forbidden` (solo `Admin` puede desactivar productos).

#### B. En Pedidos Swagger (http://localhost:5082/swagger) — Integración `Pedidos` → `Catalog`

1. **Con token de `Vendedor`** (clic en **Authorize** en http://localhost:5082/swagger → pegar `accessToken` de `Vendedor` → **Authorize**):
   - Ejecutar `POST /api/orders` con el `productId` creado en Catálogo:
     ```json
     {
       "items": [
         {
           "productId": "PEGAR_AQUI_PRODUCT_ID",
           "quantity": 2
         }
       ]
     }
     ```
     - **Resultado esperado:** `403 Forbidden` (solo el rol `Cliente` puede crear pedidos).

2. **Cambiar al token de `Cliente`** (botón **Authorize** → **Logout** → pegar `accessToken` de `Cliente` → **Authorize**):
   - Ejecutar `POST /api/orders` con el mismo JSON anterior.
     - **Resultado esperado:** `201 Created` (`Pedidos.API` reenvía el JWT del cliente a `Catalog.API` mediante `ForwardAuthorizationHandler`, obtiene el nombre y precio del producto y registra el pedido en `orders-db`). Copiar el `id` del pedido creado.
   - Ejecutar `PATCH /api/orders/{id}/status` con `{"status": "Confirmed"}`.
     - **Resultado esperado:** `403 Forbidden` (solo `Vendedor` puede cambiar el estado de un pedido).

3. **Cambiar al token de `Vendedor`** (botón **Authorize** → **Logout** → pegar `accessToken` de `Vendedor` → **Authorize**):
   - Ejecutar `PATCH /api/orders/{id}/status` con:
     ```json
     {
       "status": "Confirmed"
     }
     ```
     - **Resultado esperado:** `200 OK` con el pedido actualizado a estado `Confirmed` (`2`).

---

### Paso 5: Probar tokens inválidos o expirados (`401 Unauthorized`)

#### A. Token inválido (firma alterada o malformado)

1. En **Catalog Swagger** (http://localhost:5080/swagger) o **Pedidos Swagger** (http://localhost:5082/swagger), abrir **Authorize** → **Logout**.
2. Pegar cualquier token válido pero cambiando sus últimos caracteres (por ejemplo, reemplazar los últimos 2 caracteres por `xx`) → **Authorize**.
3. Ejecutar `GET /api/products` (o `GET /api/orders`).
   - **Resultado esperado:** `401 Unauthorized` (`www-authenticate: Bearer error="invalid_token", error_description="The signature is invalid"`).

#### B. Token expirado

**Opción 1 — Prueba inmediata en Swagger con un token ya expirado (firmado con la misma `JWT_KEY`):**

1. En **Catalog Swagger** o **Pedidos Swagger**, abrir **Authorize** → **Logout**.
2. Pegar el siguiente token (firmado con la `JWT_KEY` del proyecto, `iss: identity-service`, `aud: ecommerce-api`, pero con fecha `exp` expirada):
   ```text
   eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMTExMTExMS0xMTExLTExMTEtMTExMS0xMTExMTExMTExMTEiLCJlbWFpbCI6ImNsaWVudGVAZWNvbW1lcmNlLmxvY2FsIiwibmFtZSI6ImNsaWVudGVAZWNvbW1lcmNlLmxvY2FsIiwianRpIjoiMjIyMjIyMjItMjIyMi0yMjIyLTIyMjItMjIyMjIyMjIyMjIyIiwicm9sZSI6IkNsaWVudGUiLCJuYmYiOjE3MDAwMDAwMDAsImV4cCI6MTcwMDAwMzYwMCwiaWF0IjoxNzAwMDAwMDAwLCJpc3MiOiJpZGVudGl0eS1zZXJ2aWNlIiwiYXVkIjoiZWNvbW1lcmNlLWFwaSJ9.hJcjBXB3c7QXxSRFtXGDHm26o5QeqYvfNqRfrmHknhk
   ```
3. Ejecutar `GET /api/products` o `GET /api/orders`.
   - **Resultado esperado:** `401 Unauthorized` con cabecera `www-authenticate: Bearer error="invalid_token", error_description="The token expired at '11/14/2023 22:13:20'"`.

**Opción 2 — Emitir un token en vivo con expiración de 1 minuto:**

1. En `docker-compose.yml`, cambiar en `identity-api` la variable `Jwt__ExpirationMinutes: "1"` y aplicar el cambio:
   ```bash
   docker compose up -d --build identity-api
   ```
2. Hacer login en **Identity Swagger** (`POST /api/auth/login`), autorizarse en **Catalog Swagger** con el token obtenido y verificar que `GET /api/products` responde `200 OK`.
3. Esperar 65 segundos (`ClockSkew` está configurado en `TimeSpan.Zero`) y volver a pulsar **Execute** en `GET /api/products`.
   - **Resultado esperado:** `401 Unauthorized` por expiración del token.

---

## Script Automatizado de Pruebas y Colección de Postman

### Script de PowerShell (`pruebas-seguridad.ps1`)

En la raíz del proyecto se incluye [`pruebas-seguridad.ps1`](pruebas-seguridad.ps1), que automatiza el login con los 3 usuarios y ejecuta las validaciones de `401`, `403`, `200` y `201` en `Identity`, `Catalog` y `Pedidos`:

```powershell
# En Windows PowerShell o PowerShell Core (pwsh)
powershell -ExecutionPolicy Bypass -File .\pruebas-seguridad.ps1

# Opcional: incluyendo la espera de 70s cuando Jwt__ExpirationMinutes="1"
powershell -ExecutionPolicy Bypass -File .\pruebas-seguridad.ps1 -ProbarExpirado
```

### Colección de Postman

El archivo [`docs/Catalogo.postman_collection.json`](docs/Catalogo.postman_collection.json) incluye 39 peticiones organizadas en 5 carpetas (Autenticación en Identity, Seguridad JWT y Roles 401/403, Flujo exitoso de Catálogo, Flujo exitoso de Pedidos y Validaciones de dominio):

1. Partir de una base limpia: `docker compose down -v && docker compose up -d --build`.
2. En Postman: **Import** → seleccionar [`docs/Catalogo.postman_collection.json`](docs/Catalogo.postman_collection.json) → **Run collection**.

---

## Arquitectura del Sistema

```mermaid
flowchart TD
    subgraph Clients["Clientes y Consumidores"]
        Swagger["Swagger UI / Postman / PowerShell"]
    end

    subgraph IdentityService["Identity.API (Puerto 5081)"]
        AuthCtrl["AuthController (POST /api/auth/login)"]
        JwtSvc["JwtTokenService (HS256)"]
        Seeder["IdentitySeeder (Admin, Vendedor, Cliente)"]
        IdentityDb[("PostgreSQL 17 (identity-db:5434)")]
        AuthCtrl --> JwtSvc
        AuthCtrl --> IdentityDb
        Seeder --> IdentityDb
    end

    subgraph CatalogService["Catalog.API (Puerto 5080) - Clean Architecture + CQRS"]
        CatAuth["JWT Bearer Validation (ClockSkew = 0) & Role Authorization"]
        CatCtrl["CategoriesController & ProductsController"]
        MediatR["MediatR (Commands & Queries)"]
        CatDomain["Catalog.Domain (Product, Category, Price)"]
        CatInfra["Catalog.Infrastructure (EF Core + UnitOfWork)"]
        CatalogDb[("PostgreSQL 17 (catalog-db:5433)")]
        CatAuth --> CatCtrl --> MediatR --> CatDomain
        MediatR --> CatInfra --> CatalogDb
    end

    subgraph PedidosService["Pedidos.API (Puerto 5082) - Clean Architecture"]
        OrdAuth["JWT Bearer Validation (ClockSkew = 0) & Role Authorization"]
        OrdCtrl["OrdersController"]
        OrdSvc["OrderService"]
        FwdHandler["ForwardAuthorizationHandler (Reenvía Bearer JWT)"]
        OrdInfra["OrderRepository (EF Core)"]
        OrdersDb[("PostgreSQL 17 (orders-db:5435)")]
        OrdAuth --> OrdCtrl --> OrdSvc --> OrdInfra --> OrdersDb
        OrdSvc --> FwdHandler
    end

    Swagger -->|"1. Login (credenciales)"| AuthCtrl
    Swagger -->|"2. Bearer JWT"| CatAuth
    Swagger -->|"3. Bearer JWT"| OrdAuth
    FwdHandler -->|"4. GET /api/products/{id} (Bearer JWT)"| CatAuth
```

---

## Estructura del Proyecto

```text
├── Catalog.slnx
├── Dockerfile                         # Imagen Docker de Catalog.API
├── docker-compose.yml                 # Orquestación de los 6 contenedores
├── .env.example                       # Plantilla de variables de entorno (JWT_KEY)
├── pruebas-seguridad.ps1              # Script automatizado de pruebas de seguridad e integración
├── .config/dotnet-tools.json          # Versión fija de dotnet-ef
├── docs/
│   ├── Catalogo.postman_collection.json
│   ├── contrato-identidad.md
│   └── propuestas-microservicio-2.md
├── src/
│   ├── Identity.API/                  # ASP.NET Core Identity, EF Core, Seeders y emisión de JWT
│   ├── Catalog.Domain/                # Entidades, Value Objects y reglas de negocio del catálogo
│   ├── Catalog.Application/           # Casos de uso CQRS con MediatR y DTOs
│   ├── Catalog.Infrastructure/        # Persistencia EF Core, repositorios, Unit of Work y migraciones
│   ├── Catalog.API/                   # Controladores REST, validación JWT/Roles y Swagger con Bearer
│   ├── Pedidos.Domain/                # Entidades (Order, OrderItem) y estados del pedido
│   ├── Pedidos.Application/           # Lógica de aplicación de pedidos (OrderService) y DTOs
│   ├── Pedidos.Infrastructure/        # EF Core, cliente HTTP hacia Catalog y ForwardAuthorizationHandler
│   └── Pedidos.API/                   # Controladores REST, validación JWT/Roles y Swagger con Bearer
└── tests/
    └── Catalog.UnitTests/             # Pruebas unitarias de dominio y aplicación
```

---

## Flujo de Trabajo (Git Flow)

| Rama | Propósito |
| :--- | :--- |
| `main` | Versiones entregadas |
| `develop` | Integración del trabajo del equipo |
| `feature/*` | Una funcionalidad por rama, integrada mediante Pull Request revisado |
| `release/*` | Preparación de cada entrega |
| `hotfix/*` | Correcciones urgentes sobre `main` |

Las ramas `main` y `develop` están protegidas: solo reciben cambios por Pull Request con al menos una aprobación.
