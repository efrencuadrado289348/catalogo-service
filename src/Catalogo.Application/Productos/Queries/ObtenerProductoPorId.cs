using Catalogo.Application.Dtos;
using Catalogo.Application.Exceptions;
using Catalogo.Domain.Repositories;
using MediatR;

namespace Catalogo.Application.Productos.Queries;

public sealed record ObtenerProductoPorIdQuery(Guid Id) : IRequest<ProductoDto>;

public sealed class ObtenerProductoPorIdHandler(IProductoRepository productos)
    : IRequestHandler<ObtenerProductoPorIdQuery, ProductoDto>
{
    public async Task<ProductoDto> Handle(ObtenerProductoPorIdQuery request, CancellationToken ct)
    {
        var producto = await productos.ObtenerPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"No existe el producto con id '{request.Id}'.");

        return ProductoDto.Desde(producto);
    }
}
