using Catalogo.Application.Dtos;
using Catalogo.Domain.Repositories;
using MediatR;

namespace Catalogo.Application.Productos.Queries;

public sealed record ListarProductosQuery(Guid? CategoriaId) : IRequest<IReadOnlyList<ProductoDto>>;

public sealed class ListarProductosHandler(IProductoRepository productos)
    : IRequestHandler<ListarProductosQuery, IReadOnlyList<ProductoDto>>
{
    public async Task<IReadOnlyList<ProductoDto>> Handle(ListarProductosQuery request, CancellationToken ct)
    {
        var lista = await productos.ListarAsync(request.CategoriaId, ct);
        return lista.Select(ProductoDto.Desde).ToList();
    }
}
