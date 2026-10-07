using Catalog.Application.Abstractions;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Products.Commands;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    string? Currency,
    int Stock,
    Guid CategoryId) : IRequest<Guid>;

public sealed class CreateProductHandler(
    IProductRepository products,
    ICategoryRepository categories,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // 1. Domain rules (do not require database access)
        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.Currency,
            request.Stock,
            request.CategoryId);

        // 2. Rules requiring querying existing data
        if (!await categories.ExistsAsync(request.CategoryId, ct))
            throw new DomainException($"Category with id '{request.CategoryId}' does not exist.");

        if (await products.ExistsByNameAsync(product.Name, ct))
            throw new DomainException($"A product with the name '{product.Name}' already exists.");

        await products.AddAsync(product, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return product.Id;
    }
}
