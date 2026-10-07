using Catalog.Application.Dtos;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Categories.Queries;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;

public sealed class GetCategoryByIdHandler(ICategoryRepository categories)
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Category with id '{request.Id}' was not found.");

        return CategoryDto.From(category);
    }
}
