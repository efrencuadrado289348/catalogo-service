using Catalogo.Application.Abstractions;
using Catalogo.Application.Exceptions;
using Catalogo.Domain.Repositories;
using MediatR;

namespace Catalogo.Application.Productos.Commands;

/// <summary>Cantidad positiva = ingreso de unidades; negativa = salida.</summary>
public sealed record AjustarStockCommand(Guid ProductoId, int Cantidad) : IRequest;

public sealed class AjustarStockHandler(IProductoRepository productos, IUnitOfWork unitOfWork)
    : IRequestHandler<AjustarStockCommand>
{
    public async Task Handle(AjustarStockCommand request, CancellationToken ct)
    {
        var producto = await productos.ObtenerPorIdAsync(request.ProductoId, ct)
            ?? throw new NotFoundException($"No existe el producto con id '{request.ProductoId}'.");

        if (request.Cantidad >= 0)
            producto.AgregarStock(request.Cantidad);
        else
            producto.DescontarStock(-request.Cantidad);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
