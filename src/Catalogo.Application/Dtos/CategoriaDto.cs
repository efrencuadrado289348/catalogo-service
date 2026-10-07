using Catalogo.Domain.Entities;

namespace Catalogo.Application.Dtos;

public sealed record CategoriaDto(Guid Id, string Nombre)
{
    public static CategoriaDto Desde(Categoria c) => new(c.Id, c.Nombre);
}
