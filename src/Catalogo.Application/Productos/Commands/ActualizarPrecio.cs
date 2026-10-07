using Catalogo.Application.Abstractions;
using Catalogo.Application.Exceptions;
using Catalogo.Domain.Repositories;
using Catalogo.Domain.ValueObjects;
using MediatR;

namespace Catalogo.Application.Productos.Commands;

public sealed record ActualizarPrecioCommand(Guid ProductoId, decimal Valor, string Moneda) : IRequest;

public sealed class ActualizarPrecioHandler(IProductoRepository productos, IUnitOfWork unitOfWork)
    : IRequestHandler<ActualizarPrecioCommand>
{
    public async Task Handle(ActualizarPrecioCommand request, CancellationToken ct)
    {
        var producto = await productos.ObtenerPorIdAsync(request.ProductoId, ct)
            ?? throw new NotFoundException($"No existe el producto con id '{request.ProductoId}'.");

        producto.CambiarPrecio(Precio.Crear(request.Valor, request.Moneda));
        await unitOfWork.SaveChangesAsync(ct);
    }
}
