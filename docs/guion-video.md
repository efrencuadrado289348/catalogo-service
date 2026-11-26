# Guion del Video — MVP E-Commerce Distribuido (Identity + Catalog + Pedidos + Web)

**Duración estimada:** 4 a 6 minutos  
**Objetivo:** Demostrar el flujo completo del MVP con los 3 roles (`Admin`, `Vendedor`, `Cliente`) desde el frontend MVC y evidenciar la comunicación síncrona entre microservicios (`Web` → `Identity` / `Catalog` / `Pedidos`, y `Pedidos` → `Catalog` con propagación de JWT) mediante `docker compose logs`.

---

## Preparación previa (antes de grabar)

1. Abrir **2 ventanas** lado a lado (o alternar entre navegador y terminal):
   - **Terminal:** en la raíz del repositorio (`catalogo-service`).
   - **Navegador:** en `http://localhost:5000` (en macOS usar `http://localhost:5083` si el puerto 5000 lo ocupa AirPlay).
2. Dejar la terminal lista mostrando los contenedores activos y los logs en vivo de `pedidos-api` y `catalog-api`:
   ```bash
   docker compose ps
   docker compose logs -f pedidos-api catalog-api
   ```
3. Tener a mano las credenciales de prueba:
   - **Admin:** `admin@ecommerce.local` / `Admin123!`
   - **Vendedor:** `vendedor@ecommerce.local` / `Vendedor123!`
   - **Cliente:** `cliente@ecommerce.local` / `Cliente123!`

---

## Escena 1: Arquitectura y contenedores corriendo (~30 s)

- **En pantalla:** Terminal con `docker compose ps`.
- **Qué mostrar:** Los 7 contenedores levantados (`identity-api`, `identity-db`, `catalog-api`, `catalog-db`, `pedidos-api`, `orders-db` y `web`), destacando que cada microservicio tiene su propia base de datos PostgreSQL.
- **Narración sugerida:**
  > "Aquí tenemos nuestra arquitectura de microservicios levantada con Docker Compose: `Identity.API` para autenticación y emisión de JWT, `Catalog.API` con Clean Architecture y CQRS, `Pedidos.API` para gestión de órdenes, cada uno con su base PostgreSQL independiente, y el frontend `Web` en ASP.NET Core MVC actuando como BFF."

---

## Escena 2: Rol Admin — Gestión de categorías y productos (~60 s)

- **En pantalla:** Navegador en `/Account/Login`.
- **Acciones:**
  1. Iniciar sesión con `admin@ecommerce.local` / `Admin123!`.
  2. Mostrar la pantalla **Inicio** (`/`) con el estado `200` de `Catalog` y `Pedidos`, el badge de rol `Admin` y las opciones del menú (**Productos** y **Gestión del catálogo** — sin carrito ni gestión de pedidos).
  3. Ir a **Gestión del catálogo** (`/CatalogAdmin`) → clic en **Nueva categoría** → crear categoría (ej. *Portátiles*).
  4. Clic en **Nuevo producto** → crear un producto (ej. *ThinkPad X1 Carbon*, precio `7500000`, `COP`, stock `10`, categoría *Portátiles*).
  5. Ajustar precio o stock desde la tabla para demostrar las operaciones administrativas.
  6. Clic en **Cerrar sesión**.
- **Narración sugerida:**
  > "Ingresamos como `Admin`. En la vista inicial verificamos que el frontend se comunica de forma síncrona con `Catalog` y `Pedidos` usando el JWT emitido por `Identity`. Como `Admin` podemos crear categorías y productos, ajustar precios o inventario, y desactivar productos."

---

## Escena 3: Rol Cliente — Catálogo, carrito y creación de pedido + Evidencia en logs (~90 s)

- **En pantalla:** Navegador + Terminal con `docker compose logs -f pedidos-api catalog-api`.
- **Acciones:**
  1. Iniciar sesión con `cliente@ecommerce.local` / `Cliente123!`.
  2. Mostrar que el menú cambia según el rol: ahora aparecen **Productos**, **Mi carrito** y **Mis pedidos** (y si intenta entrar a `/CatalogAdmin` o `/SalesOrders`, recibe **Acceso denegado**).
  3. Ir a **Productos** (`/Products`), filtrar por la categoría creada, abrir **Ver detalles** y agregar `2` unidades al carrito.
  4. Ir a **Mi carrito** (`/Cart`), verificar cantidad y subtotal, y pulsar **Realizar pedido**.
  5. Mostrar la redirección a **Mis pedidos** (`/Orders/Details/{id}`) con el pedido creado en estado **Pendiente**.
  6. **Pasar a la terminal** para señalar la evidencia de comunicación síncrona en `docker logs pedidos-api`:
     ```text
     info: System.Net.Http.HttpClient.IProductCatalogService.LogicalHandler[100]
           Start processing HTTP request GET http://catalog-api:8080/api/products/<productId>
     info: System.Net.Http.HttpClient.IProductCatalogService.ClientHandler[100]
           Sending HTTP request GET http://catalog-api:8080/api/products/<productId>
     info: System.Net.Http.HttpClient.IProductCatalogService.ClientHandler[101]
           Received HTTP response headers after ...ms - 200
     ```
  7. Clic en **Cerrar sesión**.
- **Narración sugerida:**
  > "Ahora iniciamos sesión como `Cliente`. El menú oculta las vistas administrativas y habilita el carrito. Al pulsar 'Realizar pedido', `Web` envía `POST /api/orders` a `Pedidos.API`, y como vemos en `docker logs pedidos-api`, `Pedidos.API` realiza una llamada HTTP síncrona hacia `http://catalog-api:8080/api/products/{id}` reenviando el JWT del cliente mediante `ForwardAuthorizationHandler` para validar que el producto exista y obtener su nombre y precio reales desde Catálogo."

---

## Escena 4: Rol Vendedor — Gestión de pedidos y cambio de estado (~75 s)

- **En pantalla:** Navegador.
- **Acciones:**
  1. Iniciar sesión con `vendedor@ecommerce.local` / `Vendedor123!`.
  2. Mostrar el menú del rol `Vendedor`: **Productos**, **Gestión del catálogo** (puede crear productos y ajustar precio/stock, pero no crear categorías ni desactivar) y **Gestión de pedidos** (`/SalesOrders`).
  3. Entrar a **Gestión de pedidos** (`/SalesOrders`) y mostrar el listado global de pedidos con sus filtros (`Todos`, `Pendientes`, `Confirmados`, `Enviados`, `Cancelados`).
  4. En el pedido recién creado (estado **Pendiente**), pulsar **Confirmar** (`PATCH /api/orders/{id}/status` con `{"status": "Confirmed"}`) → el estado pasa a **Confirmado**.
  5. Abrir **Ver detalle** (`/SalesOrders/Details/{id}`) y pulsar **Marcar como enviado** (`PATCH /api/orders/{id}/status` con `{"status": "Shipped"}`) → el estado pasa a **Enviado** (estado final sin más transiciones).
  6. *(Opcional)* Mostrar cómo un segundo pedido en **Pendiente** o **Confirmado** puede pasar a **Cancelado** (`Cancelled`).
  7. Clic en **Cerrar sesión**.
- **Narración sugerida:**
  > "Ingresamos como `Vendedor`. En 'Gestión de pedidos' vemos todas las órdenes del sistema. A través de `PATCH /api/orders/{id}/status`, validamos la máquina de estados del dominio: de `Pending` pasamos a `Confirmed`, y de `Confirmed` a `Shipped` o `Cancelled`."

---

## Escena 5: Verificación final como Cliente y cierre (~30 s)

- **En pantalla:** Navegador.
- **Acciones:**
  1. Iniciar sesión nuevamente como `cliente@ecommerce.local` / `Cliente123!`.
  2. Ir a **Mis pedidos** (`/Orders`) y mostrar que el pedido ahora aparece con la insignia **Enviado** (o **Cancelado**).
- **Narración sugerida:**
  > "Finalmente, al volver con el `Cliente` a 'Mis pedidos', comprobamos que el estado actualizado por el `Vendedor` se refleja en tiempo real. Con esto cerramos el flujo completo del MVP con autenticación JWT, autorización por roles y comunicación síncrona entre microservicios."
