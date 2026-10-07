using Catalogo.Application.Dtos;
using Catalogo.Domain.Repositories;
using MediatR;

namespace Catalogo.Application.Categorias.Queries;

public sealed record ListarCategoriasQuery : IRequest<IReadOnlyList<CategoriaDto>>;

public sealed class ListarCategoriasHandler(ICategoriaRepository categorias)
    : IRequestHandler<ListarCategoriasQuery, IReadOnlyList<CategoriaDto>>
{
    public async Task<IReadOnlyList<CategoriaDto>> Handle(ListarCategoriasQuery request, CancellationToken ct)
    {
        var lista = await categorias.ListarAsync(ct);
        return lista.Select(CategoriaDto.Desde).ToList();
    }
}
