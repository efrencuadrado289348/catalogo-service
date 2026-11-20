# Propuestas para el segundo microservicio de negocio

La rúbrica pide un microservicio con "una responsabilidad de negocio claramente definida", protegido con JWT y autorización por roles. Los roles acordados son `Admin`, `Vendedor` y `Cliente`.

## Comparación

| Opción | Responsabilidad | Uso de roles | Relación con Catalog | Complejidad |
| :--- | :--- | :--- | :--- | :--- |
| **A. Orders (pedidos)** | Crear y consultar pedidos de compra. | `Cliente` crea y ve sus pedidos. `Vendedor` ve todos y cambia su estado. `Admin` ve todo. | Referencia productos por Id. | Media |
| **B. Reviews (reseñas)** | Opiniones y calificaciones de productos. | `Cliente` crea reseñas. `Admin` las elimina. Lectura para cualquier usuario autenticado. | Referencia productos por Id. | Baja |
| **C. Inventory (inventario y proveedores)** | Entradas de stock y proveedores. | `Vendedor` registra movimientos. `Admin` gestiona proveedores. `Cliente` sin acceso. | Referencia productos por Id. | Media |
| **D. Cart (carrito)** | Carrito de compras por usuario. | `Cliente` gestiona solo su carrito. `Admin` consulta. | Referencia productos por Id. | Baja |

## Recomendación

**Opción A, Orders.** Es la que mejor muestra los tres roles en la demostración:

- Un `Cliente` crea un pedido y solo puede ver los suyos. El servicio usa el claim `sub` para saberlo.
- Un `Vendedor` ve todos los pedidos y actualiza su estado.
- Un `Cliente` que intenta cambiar el estado de un pedido recibe **403**.

Con eso se cubren los escenarios 5, 6 y 7 del video con una historia de negocio coherente.

**Alternativa de menor riesgo: B, Reviews.** Tiene menos reglas de dominio y puede terminarse en menos tiempo, pero muestra menos diferencias entre roles.

## Nota de integración

Para no acoplar los servicios, el microservicio nuevo guarda el `ProductId` y el precio al momento de la compra. No necesita llamar a Catalog en cada solicitud. Si el equipo quiere validar que el producto existe, puede consultar `GET /api/products/{id}` en Catalog reenviando el mismo token.

La decisión final es del equipo, y el responsable es la Persona 3.
