using Catalog.Application.Dtos;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Categories.Queries;

public sealed record ListCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;

public sealed class ListCategoriesHandler(ICategoryRepository categories)
    : IRequestHandler<ListCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(ListCategoriesQuery request, CancellationToken ct)
    {
        var list = await categories.ListAsync(ct);
        return list.Select(CategoryDto.From).ToList();
    }
}
