using Catalogo.Application.Dtos;
using Catalogo.Application.Exceptions;
using Catalogo.Domain.Repositories;
using MediatR;

namespace Catalogo.Application.Categorias.Queries;

public sealed record ObtenerCategoriaPorIdQuery(Guid Id) : IRequest<CategoriaDto>;

public sealed class ObtenerCategoriaPorIdHandler(ICategoriaRepository categorias)
    : IRequestHandler<ObtenerCategoriaPorIdQuery, CategoriaDto>
{
    public async Task<CategoriaDto> Handle(ObtenerCategoriaPorIdQuery request, CancellationToken ct)
    {
        var categoria = await categorias.ObtenerPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"No existe la categoría con id '{request.Id}'.");

        return CategoriaDto.Desde(categoria);
    }
}
