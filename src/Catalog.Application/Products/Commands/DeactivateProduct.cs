using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Products.Commands;

public sealed record DeactivateProductCommand(Guid ProductId) : IRequest;

public sealed class DeactivateProductHandler(IProductRepository products, IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateProductCommand>
{
    public async Task Handle(DeactivateProductCommand request, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        product.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
