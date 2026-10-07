using Catalog.Application.Dtos;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Products.Queries;

public sealed record ListProductsQuery(Guid? CategoryId) : IRequest<IReadOnlyList<ProductDto>>;

public sealed class ListProductsHandler(IProductRepository products)
    : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(ListProductsQuery request, CancellationToken ct)
    {
        var list = await products.ListAsync(request.CategoryId, ct);
        return list.Select(ProductDto.From).ToList();
    }
}
