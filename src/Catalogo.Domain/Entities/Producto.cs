using Catalogo.Domain.Exceptions;
using Catalogo.Domain.ValueObjects;

namespace Catalogo.Domain.Entities;

/// Raíz de agregado: todo cambio de estado pasa por sus métodos,
/// que validan las reglas del negocio. Los setters son privados.
public class Producto
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }
    public Precio Precio { get; private set; }
    public int Stock { get; private set; }
    public bool Activo { get; private set; }
    public DateTime FechaCreacion { get; private set; }

    // Solo el Id: Categoria es otro agregado, no se referencia el objeto.
    public Guid CategoriaId { get; private set; }

    // Constructor
    private Producto()
    {
        Nombre = string.Empty;
        Precio = null!;
    }

    private Producto(Guid id, string nombre, string? descripcion, Precio precio, int stock, Guid categoriaId)
    {
        Id = id;
        Nombre = nombre;
        Descripcion = descripcion;
        Precio = precio;
        Stock = stock;
        CategoriaId = categoriaId;
        Activo = true;
        FechaCreacion = DateTime.UtcNow;
    }

    public static Producto Crear(
        string nombre,
        string? descripcion,
        decimal precio,
        string? moneda,
        int stock,
        Guid categoriaId)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del producto es obligatorio.");

        if (nombre.Trim().Length > 150)
            throw new DomainException("El nombre del producto no puede superar los 150 caracteres.");

        if (stock < 0)
            throw new DomainException("El stock inicial no puede ser negativo.");

        if (categoriaId == Guid.Empty)
            throw new DomainException("El producto debe pertenecer a una categoría.");

        return new Producto(
            Guid.NewGuid(),
            nombre.Trim(),
            descripcion?.Trim(),
            Precio.Crear(precio, moneda),
            stock,
            categoriaId);
    }

    public void CambiarPrecio(decimal nuevoValor, string? moneda = null)
    {
        EnsureActivo();
        Precio = Precio.Crear(nuevoValor, moneda ?? Precio.Moneda);
    }

    public void AgregarStock(int cantidad)
    {
        EnsureActivo();

        if (cantidad <= 0)
            throw new DomainException("La cantidad a agregar debe ser mayor a cero.");

        Stock += cantidad;
    }

    public void DescontarStock(int cantidad)
    {
        EnsureActivo();

        if (cantidad <= 0)
            throw new DomainException("La cantidad a descontar debe ser mayor a cero.");

        if (cantidad > Stock)
            throw new DomainException($"Stock insuficiente. Disponible: {Stock}, solicitado: {cantidad}.");

        Stock -= cantidad;
    }

    public void Desactivar()
    {
        if (!Activo)
            throw new DomainException("El producto ya está desactivado.");

        Activo = false;
    }

    private void EnsureActivo()
    {
        if (!Activo)
            throw new DomainException("No se puede modificar un producto desactivado.");
    }
}
