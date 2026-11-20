# Catálogo Service

Microservicio para la gestión de productos y categorías del e-commerce de electrónicos, implementado en **.NET 10** y **PostgreSQL**. Aplica **Clean Architecture** y **Domain-Driven Design (DDD)**, con los patrones **CQRS**, **Mediator**, **Repository** y **Unit of Work**.

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

## Gestión de Contenedores (Docker Compose)

El entorno incluye estos servicios definidos en [`docker-compose.yml`](docker-compose.yml):

| Servicio | Descripción | Puerto |
| :--- | :--- | :--- |
| `catalog-api` | API REST del catálogo | `5080` |
| `catalog-db` | PostgreSQL 17 del catálogo | `5433` |
| `identity-api` | API de Identity (usuarios, roles y Seeders) | `5081` |
| `identity-db` | PostgreSQL 17 de Identity | `5434` |

Cada API espera a que su base de datos esté lista (`healthcheck`) y aplica las migraciones automáticamente al iniciar.

### Inicio rápido

```bash
docker compose up -d --build
```

Swagger queda disponible en http://localhost:5080/swagger (Catalog) y http://localhost:5081/swagger (Identity).

### Comandos más utilizados

#### Iniciar y detener
```bash
# Iniciar servicios en segundo plano
docker compose up -d

# Reconstruir imágenes y levantar (usar tras modificar código C#)
docker compose up -d --build

# Detener los contenedores sin borrarlos
docker compose stop

# Reanudar contenedores detenidos
docker compose start

# Reiniciar servicios (o uno en específico: docker compose restart catalog-api)
docker compose restart

# Detener y remover contenedores y red
docker compose down

# Detener, remover contenedores y eliminar el volumen de datos de PostgreSQL
docker compose down -v
```

#### Monitoreo y logs
```bash
# Ver estado y mapeo de puertos de los contenedores
docker compose ps

# Ver logs en tiempo real de todos los servicios
docker compose logs -f

# Ver logs de un servicio específico
docker compose logs -f catalog-api
docker compose logs -f catalog-db
```

#### Acceso directo a contenedores
```bash
# Acceder a la consola interactiva psql de PostgreSQL
docker compose exec catalog-db psql -U catalog -d catalog

# Abrir una terminal interactiva dentro del contenedor de la API
docker compose exec catalog-api sh
```

### Ejecución local sin Docker para la API

```bash
docker compose up -d catalog-db
dotnet run --project src/Catalog.API
```

La API usa la cadena de conexión de `appsettings.json` (`localhost:5433`).

Para crear una nueva migración después de cambiar el modelo:

```bash
dotnet ef migrations add NombreMigracion \
  --project src/Catalog.Infrastructure \
  --startup-project src/Catalog.API \
  --output-dir Persistence/Migrations
```

---

## Identity (`Identity.API`)

Microservicio de usuarios y roles con **ASP.NET Core Identity** y **Entity Framework Core** sobre PostgreSQL. No expone CRUD de usuarios ni de roles: los datos iniciales los crean los Seeders. El contrato completo (roles, JWT y claims) está en [`docs/contrato-identidad.md`](docs/contrato-identidad.md).

### Usuarios y roles iniciales

Al iniciar en entorno `Development`, `Identity.API` aplica la migración y ejecuta el Seeder. Es idempotente: si los datos ya existen, no los duplica ni cambia las contraseñas.

| Correo | Rol | Contraseña (solo desarrollo) |
| :--- | :--- | :--- |
| `admin@ecommerce.local` | `Admin` | `Admin123!` |
| `vendedor@ecommerce.local` | `Vendedor` | `Vendedor123!` |
| `cliente@ecommerce.local` | `Cliente` | `Cliente123!` |

Los correos están en `appsettings.json` y las contraseñas en `appsettings.Development.json` (sección `SeedUsers`). Se pueden sobrescribir con variables de entorno, por ejemplo `SeedUsers__Admin__Password`. Las contraseñas se guardan con el hasher de ASP.NET Core Identity, nunca en texto plano.

### Verificar los Seeders

```bash
docker compose up -d --build identity-db identity-api

# Logs del Seeder
docker compose logs identity-api | grep "Seed:"

# Usuarios y roles en la base de datos
docker compose exec identity-db psql -U identity -d identity -c \
  'SELECT u."Email", r."Name" AS rol FROM "AspNetUsers" u
   JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id"
   JOIN "AspNetRoles" r ON r."Id" = ur."RoleId" ORDER BY r."Name";'
```

### Ejecución local y migraciones

```bash
docker compose up -d identity-db
dotnet run --project src/Identity.API
```

La herramienta `dotnet-ef` está fijada en [`.config/dotnet-tools.json`](.config/dotnet-tools.json). Se instala con `dotnet tool restore` y funciona igual en Windows, Linux y macOS:

```bash
dotnet tool restore
dotnet ef migrations add NombreMigracion --project src/Identity.API --output-dir Data/Migrations
```

> **Nota:** en la primera ejecución sobre una base vacía, el log muestra un `fail` sobre la tabla `__EFMigrationsHistory`. Es normal: EF Core la consulta antes de crearla.

---

## Documentación de Endpoints

- **URL Base:** `http://localhost:5080`
- **Swagger UI:** `http://localhost:5080/swagger` (disponible en entorno `Development`)

---

### Categorías (`/api/categories`)

| Método | Ruta | Descripción | Respuestas |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/categories` | Crea una nueva categoría | `201 Created`, `400 Bad Request` |
| `GET` | `/api/categories` | Lista todas las categorías | `200 OK` |
| `GET` | `/api/categories/{id}` | Obtiene una categoría por su ID (`Guid`) | `200 OK`, `404 Not Found` |

#### Payload: Crear categoría (`POST /api/categories`)
```json
{
  "name": "Electrónica"
}
```

---

### Productos (`/api/products`)

| Método | Ruta | Descripción | Respuestas |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/products` | Crea un nuevo producto | `201 Created`, `400 Bad Request` |
| `GET` | `/api/products` | Lista productos (filtro opcional: `?categoriaId={guid}`) | `200 OK` |
| `GET` | `/api/products/{id}` | Obtiene un producto por su ID (`Guid`) | `200 OK`, `404 Not Found` |
| `PATCH` | `/api/products/{id}/price` | Actualiza el precio y moneda | `204 No Content`, `400`, `404` |
| `PATCH` | `/api/products/{id}/stock` | Ajusta el inventario (suma o resta) | `204 No Content`, `400`, `404` |
| `PATCH` | `/api/products/{id}/deactivate` | Marca el producto como inactivo | `204 No Content`, `400`, `404` |

#### Payload: Crear producto (`POST /api/products`)
```json
{
  "name": "Teclado Mecánico",
  "description": "Teclado inalámbrico RGB",
  "price": 89.99,
  "currency": "USD",
  "stock": 25,
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

#### Payload: Actualizar precio (`PATCH /api/products/{id}/price`)
```json
{
  "amount": 79.99,
  "currency": "USD"
}
```

#### Payload: Ajustar stock (`PATCH /api/products/{id}/stock`)
```json
{
  "quantity": 10
}
```
> **Nota:** Para restar inventario se envían valores negativos (ej. `"quantity": -5`).

---

### Colección de Postman

El archivo [`docs/Catalogo.postman_collection.json`](docs/Catalogo.postman_collection.json) tiene 22 peticiones: 9 del flujo exitoso y 13 de errores de validación de dominio. Para ejecutarla:

1. Partir de una base limpia: `docker compose down -v && docker compose up -d --build`.
2. En Postman: **Import** → seleccionar el archivo → **Run collection**.

### Formato de errores

Los errores se devuelven en formato **ProblemDetails** (RFC 9457). Ejemplo al crear una categoría duplicada:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Regla de negocio violada",
  "status": 400,
  "detail": "Ya existe una categoría con el nombre 'Prueba'."
}
```

| Código | Cuándo ocurre |
| :--- | :--- |
| `400` | Se viola una regla de negocio del dominio |
| `404` | El recurso solicitado no existe |
| `500` | Error inesperado (sin exponer detalles internos) |

---

## Arquitectura del Sistema

El proyecto implementa **Clean Architecture**: las dependencias apuntan hacia el dominio, que no depende de ninguna otra capa. La capa de aplicación usa **CQRS** con **MediatR**, manteniendo el núcleo de negocio desacoplado de la infraestructura y la presentación.

```mermaid
flowchart TD
    subgraph Clients["Clientes y Consumidores"]
        Client["Cliente HTTP / Postman / Swagger UI"]
    end

    subgraph API["Catalog.API (Presentación)"]
        Controllers["Controllers (CategoriesController, ProductsController)"]
        ExceptionHandling["GlobalExceptionHandler & ProblemDetails"]
    end

    subgraph Application["Catalog.Application (Casos de Uso - CQRS)"]
        MediatR["MediatR (ISender)"]
        Commands["Commands & Handlers (Crear, UpdatePrice, AdjustStock, Desactivar)"]
        Queries["Queries & Handlers (Listar, ObtenerPorId)"]
        Abstractions["IUnitOfWork"]
        Dtos["DTOs (ProductDto, CategoryDto)"]
    end

    subgraph Domain["Catalog.Domain (Núcleo de Dominio)"]
        Entities["Entidades (Product, Category)"]
        ValueObjects["Value Objects (Price)"]
        DomainExceptions["Domain Exceptions"]
        RepoInterfaces["Interfaces de Repositorio (IProductRepository, ICategoryRepository)"]
    end

    subgraph Infrastructure["Catalog.Infrastructure (Persistencia & Datos)"]
        DbContext["EF Core CatalogDbContext"]
        RepoImpl["Repositorios (ProductRepository, CategoryRepository)"]
        UoWImpl["UnitOfWork"]
        Migrations["Migraciones automáticas al iniciar"]
    end

    subgraph Database["Almacenamiento (Contenedor Docker)"]
        Postgres[("PostgreSQL 17 (catalog-db:5432)")]
    end

    %% Flujo de peticiones
    Client -->|"HTTP Requests (Puerto 5080)"| Controllers
    Controllers --> ExceptionHandling
    Controllers -->|"Envía Command / Query"| MediatR
    MediatR --> Commands
    MediatR --> Queries

    %% Capa de Aplicación interactuando con Dominio
    Commands -->|"Crea / Modifica"| Entities
    Commands -->|"Valida con"| ValueObjects
    Commands -->|"Invoca"| RepoInterfaces
    Commands -->|"Confirma cambios con"| Abstractions
    Queries -->|"Consulta vía"| RepoInterfaces
    Queries -->|"Devuelve"| Dtos

    %% Inversión de dependencias (Infraestructura implementa interfaces)
    RepoImpl -.->|"Implementa"| RepoInterfaces
    UoWImpl -.->|"Implementa"| Abstractions
    RepoImpl --> DbContext
    UoWImpl --> DbContext
    DbContext -->|"Npgsql"| Postgres
```

### Patrones aplicados

- **DDD:** `Product` es la raíz de agregado y protege sus reglas (precio mayor a cero, stock no negativo, no modificar productos desactivados). `Price` es un value object inmutable. `Product` referencia a `Category` solo por su Id.
- **CQRS:** las operaciones que escriben (Commands) están separadas de las que leen (Queries).
- **Mediator:** los controllers solo envían Commands y Queries con `ISender` de MediatR; no conocen a los handlers.
- **Repository:** las interfaces están en el dominio y las implementaciones en infraestructura.
- **Unit of Work:** los repositorios solo agregan o modifican; `IUnitOfWork.SaveChangesAsync` confirma todo en una sola transacción.

---

## Estructura del Proyecto

```text
├── Catalog.slnx
├── Dockerfile                   # Imagen de Catalog.API
├── docker-compose.yml
├── .config/dotnet-tools.json    # Versión fija de dotnet-ef
├── docs/
│   ├── Catalogo.postman_collection.json
│   ├── contrato-identidad.md
│   └── propuestas-microservicio-2.md
└── src/
    ├── Identity.API/            # ASP.NET Core Identity, EF Core, migraciones y Seeders (con su Dockerfile)
    ├── Catalog.Domain/          # Entidades, Value Objects, excepciones y reglas de negocio
    ├── Catalog.Application/     # Casos de uso (Commands/Queries con MediatR) y DTOs
    ├── Catalog.Infrastructure/  # Persistencia con EF Core, repositorios, Unit of Work y migraciones
    └── Catalog.API/             # Controladores REST, manejo de errores y configuración de inicio
```

---

## Flujo de trabajo (Git Flow)

| Rama | Propósito |
| :--- | :--- |
| `main` | Versiones entregadas (tag `v1.0.0`) |
| `develop` | Integración del trabajo del equipo |
| `feature/*` | Una funcionalidad por rama, integrada mediante Pull Request revisado |
| `release/*` | Preparación de cada entrega |
| `hotfix/*` | Correcciones urgentes sobre `main` |

Las ramas `main` y `develop` están protegidas: solo reciben cambios por Pull Request con al menos una aprobación.
