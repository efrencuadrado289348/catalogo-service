using Catalogo.Domain.Exceptions;

namespace Catalogo.Domain.Entities;

public class Categoria
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }

    // Constructor
    private Categoria()
    {
        Nombre = string.Empty;
    }

    private Categoria(Guid id, string nombre, string? descripcion)
    {
        Id = id;
        Nombre = nombre;
        Descripcion = descripcion;
    }

    public static Categoria Crear(string nombre, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre de la categoría es obligatorio.");

        if (nombre.Trim().Length > 100)
            throw new DomainException("El nombre de la categoría no puede superar los 100 caracteres.");

        return new Categoria(Guid.NewGuid(), nombre.Trim(), descripcion?.Trim());
    }
}
