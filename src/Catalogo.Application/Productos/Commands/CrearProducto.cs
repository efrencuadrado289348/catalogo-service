using Catalogo.Application.Abstractions;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Exceptions;
using Catalogo.Domain.Repositories;
using Catalogo.Domain.ValueObjects;
using MediatR;

namespace Catalogo.Application.Productos.Commands;

public sealed record CrearProductoCommand(
    string Nombre,
    string? Descripcion,
    decimal Precio,
    string Moneda,
    int Stock,
    Guid CategoriaId) : IRequest<Guid>;

public sealed class CrearProductoHandler(
    IProductoRepository productos,
    ICategoriaRepository categorias,
    IUnitOfWork unitOfWork) : IRequestHandler<CrearProductoCommand, Guid>
{
    public async Task<Guid> Handle(CrearProductoCommand request, CancellationToken ct)
    {
        // 1. Reglas del dominio (no requieren base de datos)
        var producto = Producto.Crear(
            request.Nombre,
            request.Descripcion,
            request.Precio,
            request.Moneda,
            request.Stock,
            request.CategoriaId);

        // 2. Reglas que requieren consultar datos existentes
        if (!await categorias.ExisteAsync(request.CategoriaId, ct))
            throw new DomainException($"La categoría con id '{request.CategoriaId}' no existe.");

        if (await productos.ExisteNombreAsync(producto.Nombre, ct))
            throw new DomainException($"Ya existe un producto con el nombre '{producto.Nombre}'.");

        await productos.AgregarAsync(producto, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return producto.Id;
    }
}
