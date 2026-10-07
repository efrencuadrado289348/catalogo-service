using Catalogo.Domain.Entities;

namespace Catalogo.Application.Dtos;

public sealed record ProductoDto(
    Guid Id,
    string Nombre,
    string Descripcion,
    decimal Precio,
    string Moneda,
    int Stock,
    Guid CategoriaId,
    bool Activo)
{
    public static ProductoDto Desde(Producto p) =>
        new(p.Id, p.Nombre, p.Descripcion, p.Precio.Valor, p.Precio.Moneda, p.Stock, p.CategoriaId, p.Activo);
}
