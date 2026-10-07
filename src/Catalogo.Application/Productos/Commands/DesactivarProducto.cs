using Catalogo.Application.Abstractions;
using Catalogo.Application.Exceptions;
using Catalogo.Domain.Repositories;
using MediatR;

namespace Catalogo.Application.Productos.Commands;

public sealed record DesactivarProductoCommand(Guid ProductoId) : IRequest;

public sealed class DesactivarProductoHandler(IProductoRepository productos, IUnitOfWork unitOfWork)
    : IRequestHandler<DesactivarProductoCommand>
{
    public async Task Handle(DesactivarProductoCommand request, CancellationToken ct)
    {
        var producto = await productos.ObtenerPorIdAsync(request.ProductoId, ct)
            ?? throw new NotFoundException($"No existe el producto con id '{request.ProductoId}'.");

        producto.Desactivar();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
