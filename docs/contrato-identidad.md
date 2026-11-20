# Contrato de Identity (segunda entrega)

Este documento fija los acuerdos entre las cinco personas del equipo. Si algo cambia, se actualiza aquí en la misma Pull Request.

## Servicios y puertos

| Servicio | Puerto API (host) | Puerto BD (host) | Base de datos |
| :--- | :--- | :--- | :--- |
| `catalog-api` (primera entrega) | 5080 | 5433 | `catalog` |
| `identity-api` | 5081 | 5434 | `identity` |
| Segundo microservicio de negocio | 5082 | 5435 | por definir |

Dentro de la red de Docker los servicios se llaman por su nombre (`identity-api:8080`, `catalog-api:8080`).

## Roles

| Rol | Descripción |
| :--- | :--- |
| `Admin` | Administra todo el sistema. |
| `Vendedor` | Gestiona productos y pedidos del negocio. |
| `Cliente` | Consume el catálogo y gestiona sus propios recursos. |

Los nombres son sensibles a mayúsculas. Se usan tal cual en `[Authorize(Roles = "Admin,Vendedor")]`.

## Usuarios iniciales (Seeders)

Los crea `Identity.API` al iniciar. Son idempotentes: si ya existen, no se duplican.

| Correo | Rol | Contraseña (solo desarrollo) |
| :--- | :--- | :--- |
| `admin@ecommerce.local` | `Admin` | `Admin123!` |
| `vendedor@ecommerce.local` | `Vendedor` | `Vendedor123!` |
| `cliente@ecommerce.local` | `Cliente` | `Cliente123!` |

Las contraseñas se leen de la configuración (`SeedUsers:*:Password`) y se guardan con el hasher de ASP.NET Core Identity (PBKDF2). Nunca se guardan en texto plano. Estos valores son solo para el entorno de desarrollo y la demostración.

Para la prueba de HTTP 403, el usuario `cliente@ecommerce.local` está autenticado pero no tiene los roles `Admin` ni `Vendedor`.

## JWT

| Parámetro | Valor |
| :--- | :--- |
| Algoritmo | `HS256` (clave simétrica compartida) |
| `Jwt:Issuer` | `identity-service` |
| `Jwt:Audience` | `ecommerce-api` |
| `Jwt:Key` | Variable de entorno `Jwt__Key`, mínimo 32 caracteres. En desarrollo, el valor está en `docker-compose.yml`. |
| `Jwt:ExpirationMinutes` | `60` (configurable con `Jwt__ExpirationMinutes`; para la demostración del token expirado se usa un valor de 1) |

### Claims del token

| Claim | Contenido |
| :--- | :--- |
| `sub` | Id del usuario (Guid). Sirve para saber a quién pertenece un recurso. |
| `email` | Correo del usuario. |
| `name` | Nombre de usuario. |
| `role` | Un claim `role` por cada rol del usuario. |
| `jti` | Id único del token. |
| `iss`, `aud`, `iat`, `exp` | Estándar. |

### Validación en los otros microservicios

Cada servicio protegido debe validar firma, emisor, audiencia y vigencia. No basta con decodificar el token.

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // conserva "role" y "sub" tal como vienen
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,   // el token expira exactamente en `exp`
            RoleClaimType = "role",
            NameClaimType = "name"
        };
    });
builder.Services.AddAuthorization();
// ...
app.UseAuthentication();
app.UseAuthorization();
```

- Sin token o con token inválido o expirado: **401**.
- Token válido sin el rol requerido: **403**.

## Login (Persona 2)

`POST /api/auth/login` en `identity-api`.

Solicitud:

```json
{ "email": "admin@ecommerce.local", "password": "Admin123!" }
```

Respuesta `200 OK`:

```json
{ "accessToken": "<jwt>", "tokenType": "Bearer", "expiresIn": 3600 }
```

Credenciales inválidas: `401 Unauthorized`.

## Matriz de acceso propuesta para Catalog (Persona 4)

| Endpoint | Acceso |
| :--- | :--- |
| `GET /api/categories`, `GET /api/products` y por Id | Cualquier usuario autenticado |
| `POST /api/products`, `PATCH .../price`, `PATCH .../stock` | `Admin`, `Vendedor` |
| `POST /api/categories`, `PATCH .../deactivate` | `Admin` |

## Responsabilidades de cada parte en `Identity.API`

- **Persona 1:** `ApplicationUser`, `IdentityDbContext`, migraciones, Seeders y la configuración de ASP.NET Core Identity.
- **Persona 2:** endpoint de Login, servicio de generación del JWT y configuración `Jwt:*`.
