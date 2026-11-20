using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Products.Commands;

/// <summary>Positive quantity = incoming units; negative = outgoing units.</summary>
public sealed record AdjustStockCommand(Guid ProductId, int Quantity) : IRequest;

public sealed class AdjustStockHandler(IProductRepository products, IUnitOfWork unitOfWork)
    : IRequestHandler<AdjustStockCommand>
{
    public async Task Handle(AdjustStockCommand request, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        if (request.Quantity >= 0)
            product.AddStock(request.Quantity);
        else
            product.DeductStock(-request.Quantity);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
