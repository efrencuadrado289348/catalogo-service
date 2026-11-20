using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Products.Commands;

public sealed record UpdatePriceCommand(Guid ProductId, decimal Amount, string? Currency = null) : IRequest;

public sealed class UpdatePriceHandler(IProductRepository products, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePriceCommand>
{
    public async Task Handle(UpdatePriceCommand request, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        product.UpdatePrice(request.Amount, request.Currency);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
