using Catalog.Application.Dtos;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Products.Queries;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public sealed class GetProductByIdHandler(IProductRepository products)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Product with id '{request.Id}' was not found.");

        return ProductDto.From(product);
    }
}
